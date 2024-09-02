Imports System.IO

Public Class ScpDecoder
    Private Const TRACK_ADJUST_RANGE = 20

    Private m_scpFile As ScpFile

    Public Sub New(scpFile As ScpFile)
        m_scpFile = scpFile
    End Sub

    Public Function DecodeMfm(diskType As FloppyDiskType, rotationFixup As Boolean) As MfmImage
        Dim fileName = Path.GetFileName(m_scpFile.FilePath)
        Dim mfmImage = New MfmImage(fileName, diskType)

        Dim maxTracks = (ScpFile.MAX_TRACK_NUMBER + 1) \ 2
        For trackNumber = 0 To maxTracks
            Dim side0TrackNumber = trackNumber * 2
            Dim side1TrackNumber = trackNumber * 2 + 1

            Dim mfmTrack = New MfmTrack(trackNumber)
            Dim foundSide0Track = ProcessTrack(side0TrackNumber, diskType, mfmTrack, 0, rotationFixup)
            Dim foundSide1Track = ProcessTrack(side1TrackNumber, diskType, mfmTrack, 1, rotationFixup)

            If foundSide0Track OrElse foundSide1Track Then
                mfmImage.Tracks.Add(mfmTrack)
            End If
        Next

        Return mfmImage
    End Function

    Private Function ProcessTrack(scpTrackNumber As Integer, diskType As FloppyDiskType, mfmTrack As MfmTrack, side As Integer, rotationFixup As Boolean) As Boolean
        Dim scpTrack = m_scpFile.Tracks.FirstOrDefault(Function(t) t.TrackOffset.TrackNumber = scpTrackNumber)
        If scpTrack Is Nothing Then
            Return False
        End If

        ' Run through all the revolutions once to get the decoder timing synchronized

        Dim timingList = New List(Of List(Of Double))
        Dim fluxDecoder = New MfmFluxDecoder(diskType)
        'Console.WriteLine($"Nominal Bitcell Time: {fluxDecoder.NominalBitcellTime}")
        For Each revolution In scpTrack.TrackRevolutionData
            Dim timings = NormalizeTiming(revolution.TimeEntries, m_scpFile.Header.CaptureResolutionInSeconds)
            timingList.Add(timings)

            Dim revolutionBitList = fluxDecoder.Decode(timings, mfmTrack.TrackNumber, side)
            'Console.WriteLine($"Current Bitcell Time: {fluxDecoder.CurrentBitcellTime}")
        Next

        Dim revolutionBitLists = New List(Of BitList)
        Dim completeReadBitList = New BitList()
        For Each timings In timingList
            Dim revolutionBitList = fluxDecoder.Decode(timings, mfmTrack.TrackNumber, side)

            If rotationFixup Then
                completeReadBitList.AddBitList(revolutionBitList)
            End If

            revolutionBitLists.Add(revolutionBitList)
            'Console.WriteLine($"Current Bitcell Time: {fluxDecoder.CurrentBitcellTime}")
        Next

        'Console.WriteLine($"Track Number: {mfmTrack.TrackNumber}, Side: {side}")

        Dim revolutions = New List(Of MfmTrackRevolution)
        Dim currentIndex = 0
        For i = 0 To revolutionBitLists.Count - 1
            Dim revolutionBitList = revolutionBitLists(i)

            Dim fixupsApplied = False
            If rotationFixup AndAlso i > 0 AndAlso i < revolutionBitLists.Count - 1 Then
                Dim slice = revolutionBitList.Slice(0, revolutionBitList.BitCount / 4)

                Dim expectedIndex = currentIndex + revolutionBitList.BitCount
                Dim nextIndex = completeReadBitList.Find(expectedIndex - TRACK_ADJUST_RANGE, expectedIndex + TRACK_ADJUST_RANGE, slice)
                If nextIndex <> -1 Then
                    Dim difference = nextIndex - expectedIndex
                    If difference <> 0 Then
                        Dim nextRevolutionBitList = revolutionBitLists(i + 1)
                        If difference > 0 Then
                            'Console.WriteLine($"Move {difference} bits from start of revolution {i + 1} to end of revolution {i}")
                            revolutionBitList.ShiftBitsFromStartOf(nextRevolutionBitList, difference)
                            fixupsApplied = True
                        ElseIf difference < 0 Then
                            difference = Math.Abs(difference)
                            'Console.WriteLine($"Move {difference} bits from end of revolution {i} to start of revolution {i + 1}")
                            revolutionBitList.ShiftBitsToStartOf(nextRevolutionBitList, difference)
                            fixupsApplied = True
                        End If
                    End If
                End If
            End If

            currentIndex += revolutionBitList.BitCount
            Dim decoder = New MfmTrackRevolutionDecoder(revolutionBitList, i, fixupsApplied)
            Dim decodedRevolution = decoder.Decode()
            decodedRevolution.TimeEntries = timingList(i)

            revolutions.Add(decodedRevolution)
            mfmTrack.AddRevolution(decodedRevolution, side)
        Next

        For i = 1 To revolutions.Count
            Dim revolutionGroup = i
            Dim exampleRevolution = revolutions.FirstOrDefault(Function(g) g.RevolutionGroup = revolutionGroup)
            If exampleRevolution Is Nothing Then
                Continue For
            End If

            For j = i To revolutions.Count - 1
                Dim testRevolution = revolutions(j)
                If testRevolution.RevolutionGroup <= i Then
                    Continue For
                End If

                Dim percentage = exampleRevolution.MatchPercentage(testRevolution)
                If percentage > 0.99 Then
                    testRevolution.RevolutionGroup = i
                End If
            Next
        Next

        Dim groupedGroups = revolutions.GroupBy(Function(g) g.RevolutionGroup)
        Dim bestGroup = FindBestGroup(groupedGroups)

        If bestGroup IsNot Nothing Then
            Dim bestRevolution = FindBestRevolutionInGroup(bestGroup)
            If bestRevolution IsNot Nothing Then
                mfmTrack.SelectRevolution(bestRevolution.RevolutionNumber, side)
            Else
                Throw New InvalidOperationException($"Unable to find best revolution for scp track number: {scpTrackNumber}")
            End If
        Else
            Throw New InvalidOperationException($"Unable to find best revolution group for scp track number: {scpTrackNumber}")
        End If

        Return True
    End Function

    Private Function FindBestGroup(groups As IEnumerable(Of IGrouping(Of Integer, MfmTrackRevolution))) As IGrouping(Of Integer, MfmTrackRevolution)
        Dim largestGroup = groups.Max(Function(g) g.Count)
        Dim largeGroups = groups.Where(Function(g) g.Count = largestGroup)
        If largeGroups.Count = 1 Then
            Dim group = largeGroups(0)
            Return group
        Else
            Dim validGroups = largeGroups.Where(Function(g) g.Any(Function(r) r.SectorsValid))
            If validGroups.Count = 1 Then
                Dim group = validGroups(0)
                Return group
            ElseIf validGroups.Count > 1 Then
                Dim fixupGroups = validGroups.Where(Function(g) g.Any(Function(r) r.FixupsApplied))
                If fixupGroups.Count = 1 Then
                    Dim group = fixupGroups(0)
                    Return group
                ElseIf fixupGroups.Count > 1 Then
                    Dim highestGroup = fixupGroups.OrderByDescending(Function(vfg) vfg.Key).FirstOrDefault()
                    Return highestGroup
                Else
                    Dim highestGroup = validGroups.OrderByDescending(Function(vg) vg.Key).FirstOrDefault()
                    Return highestGroup
                End If
            Else
                Dim fixupGroups = largeGroups.Where(Function(g) g.Any(Function(r) r.FixupsApplied))
                If fixupGroups.Count = 1 Then
                    Dim group = fixupGroups(0)
                    Return group
                ElseIf fixupGroups.Count > 1 Then
                    Dim highestGroup = fixupGroups.OrderByDescending(Function(fg) fg.Key).FirstOrDefault()
                    Return highestGroup
                Else
                    Dim highestGroup = largeGroups.OrderByDescending(Function(g) g.Key).FirstOrDefault()
                    Return highestGroup
                End If
            End If
        End If
    End Function

    Private Function FindBestRevolutionInGroup(group As IGrouping(Of Integer, MfmTrackRevolution)) As MfmTrackRevolution
        If group.Count = 1 Then
            Return group(0)
        Else
            Dim validRevolutions = group.Where(Function(r) r.SectorsValid)
            If validRevolutions.Count = 1 Then
                Return validRevolutions(0)
            ElseIf validRevolutions.Count > 1 Then
                Dim revolutionsWithFixups = validRevolutions.Where(Function(vr) vr.FixupsApplied)
                If revolutionsWithFixups.Count = 1 Then
                    Return revolutionsWithFixups(0)
                ElseIf revolutionsWithFixups.Count > 1 Then
                    Dim lastRevolution = revolutionsWithFixups.OrderByDescending(Function(vfr) vfr.RevolutionNumber).FirstOrDefault()
                    Return lastRevolution
                Else
                    Dim lastRevolution = validRevolutions.OrderByDescending(Function(vr) vr.RevolutionNumber).FirstOrDefault()
                    Return lastRevolution
                End If
            Else
                Dim revolutionsWithFixups = group.Where(Function(r) r.FixupsApplied)
                If revolutionsWithFixups.Count = 1 Then
                    Return revolutionsWithFixups(0)
                ElseIf revolutionsWithFixups.Count > 1 Then
                    Dim lastRevolution = revolutionsWithFixups.OrderByDescending(Function(fr) fr.RevolutionNumber).FirstOrDefault()
                    Return lastRevolution
                Else
                    Dim lastRevolution = group.OrderByDescending(Function(fr) fr.RevolutionNumber).FirstOrDefault()
                    Return lastRevolution
                End If
            End If
        End If
    End Function

    Private Function NormalizeTiming(timeEntries As List(Of UShort), tickResolution As Double) As List(Of Double)
        Dim timing = New List(Of Double)

        Dim accumulatedTicks As Long = 0
        For Each entry In timeEntries
            If entry = 0 Then
                accumulatedTicks += 65536
                Continue For
            End If
            accumulatedTicks += entry

            timing.Add(accumulatedTicks * tickResolution)
            accumulatedTicks = 0
        Next

        Return timing
    End Function
End Class
