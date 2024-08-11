Public Class ScpDecoder
    Private Const TRACK_ADJUST_RANGE = 20

    Private m_scpFile As ScpFile

    Public Sub New(scpFile As ScpFile)
        m_scpFile = scpFile
    End Sub

    Public Function DecodeMfm(diskType As FloppyDiskType, rotationFixup As Boolean) As MfmImage
        Dim mfmImage = New MfmImage(diskType)

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

        Dim timingList = New List(Of List(Of Long))
        Dim fluxDecoder = New MfmFluxDecoder(diskType, m_scpFile.Header.CaptureResolutionInSeconds)
        'Console.WriteLine($"Nominal Bitcell Time: {fluxDecoder.NominalBitcellTime}")
        For Each revolution In scpTrack.TrackRevolutionData
            Dim timings = NormalizeTiming(revolution.TimeEntries)
            timingList.Add(timings)

            Dim revolutionBitList = fluxDecoder.Decode(timings)
            'Console.WriteLine($"Current Bitcell Time: {fluxDecoder.CurrentBitcellTime}")
        Next

        Dim revolutionBitLists = New List(Of BitList)
        Dim completeReadBitList = New BitList()
        For Each timings In timingList
            Dim revolutionBitList = fluxDecoder.Decode(timings)

            If rotationFixup Then
                completeReadBitList.AddBitList(revolutionBitList)
            End If

            revolutionBitLists.Add(revolutionBitList)
            'Console.WriteLine($"Current Bitcell Time: {fluxDecoder.CurrentBitcellTime}")
        Next

        'Console.WriteLine($"Track Number: {mfmTrack.TrackNumber}, Side: {side}")

        If rotationFixup Then
            Dim currentIndex = 0
            For i = 0 To revolutionBitLists.Count - 2
                Dim revolutionBitList = revolutionBitLists(i)

                If i > 0 Then
                    Dim slice = revolutionBitList.Slice(0, revolutionBitList.BitCount / 4)

                    Dim expectedIndex = currentIndex + revolutionBitList.BitCount
                    Dim nextIndex = completeReadBitList.Find(expectedIndex - TRACK_ADJUST_RANGE, expectedIndex + TRACK_ADJUST_RANGE, slice)
                    If nextIndex <> -1 Then
                        Dim difference = nextIndex - expectedIndex
                        If difference = 0 Then
                            Continue For
                        End If

                        Dim nextRevolutionBitList = revolutionBitLists(i + 1)
                        If difference > 0 Then
                            'Console.WriteLine($"Move {difference} bits from start of revolution {i + 1} to end of revolution {i}")
                            revolutionBitList.ShiftBitsFromStartOf(nextRevolutionBitList, difference)
                        ElseIf difference < 0 Then
                            difference = Math.Abs(difference)
                            'Console.WriteLine($"Move {difference} bits from end of revolution {i} to start of revolution {i + 1}")
                            revolutionBitList.ShiftBitsToStartOf(nextRevolutionBitList, difference)
                        End If
                    End If
                End If

                currentIndex += revolutionBitList.BitCount
            Next
        End If

        Dim revolutions = New List(Of MfmTrackRevolution)
        For Each revolutionBitList In revolutionBitLists
            Dim decoder = New MfmTrackRevolutionDecoder(revolutionBitList)
            Dim decodedRevolution = decoder.Decode()

            revolutions.Add(decodedRevolution)
            mfmTrack.AddRevolution(decodedRevolution, side)
        Next

        Dim revGroups = New Dictionary(Of Integer, Integer)(revolutions.Count)

        For i = 0 To revolutions.Count - 1
            revGroups(i) = i + 1
        Next

        For i = 1 To revolutions.Count
            Dim revolutionNumber = i
            Dim index = revGroups.FirstOrDefault(Function(g) g.Value = revolutionNumber)
            If index.Value = 0 Then
                Continue For
            End If
            Dim baseRevolution = revolutions(index.Key)

            For j = i To revolutions.Count - 1
                If revGroups(j) <= i Then
                    Continue For
                End If

                Dim testRevolution = revolutions(j)
                Dim percentage = baseRevolution.MatchPercentage(testRevolution)
                If percentage > 0.95 Then
                    revGroups(j) = i
                End If
            Next
        Next

        Dim groupedGroups = revGroups.GroupBy(Function(g) g.Value)
        Dim largestGroup = groupedGroups.Max(Function(g) g.Count)
        Dim largeGroups = groupedGroups.Where(Function(g) g.Count = largestGroup)
        If largeGroups.Count = 1 Then
            Dim group = largeGroups(0)
            If group.Count = 1 Then
                Dim selectedIndex = group(0).Key
                'Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, only one in largest group")
                mfmTrack.SelectRevolution(selectedIndex, side)
            Else
                Dim validRevolutions = group.Where(Function(r) revolutions(r.Key).IsValid)
                If validRevolutions.Count = 1 Then
                    Dim selectedIndex = validRevolutions(0).Key
                    'Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, only valid one in largest group")
                    mfmTrack.SelectRevolution(selectedIndex, side)
                ElseIf validRevolutions.Count = 0 Then
                    Dim selectedIndex = group.OrderByDescending(Function(g) DistanceToEnd(g.Key, revolutions.Count)).First().Key
                    'Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, Middle one in largest group")
                    mfmTrack.SelectRevolution(selectedIndex, side)
                Else
                    Dim selectedIndex = validRevolutions.OrderByDescending(Function(g) DistanceToEnd(g.Key, revolutions.Count)).First().Key
                    ' Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, Middle valid one in largest group")
                    mfmTrack.SelectRevolution(selectedIndex, side)
                End If
            End If
        Else
            Dim validGroups = groupedGroups.Where(Function(g) g.Count = largestGroup AndAlso revolutions(g.First().Key).IsValid)
            If validGroups.Count = 1 Then
                Dim selectedIndex = validGroups(0).OrderByDescending(Function(g) DistanceToEnd(g.Key, revolutions.Count)).First().Key
                'Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, Middle one in largest valid group")
                mfmTrack.SelectRevolution(selectedIndex, side)
            ElseIf validGroups.Count = 0 Then
                Dim selectedIndex = groupedGroups.Where(Function(g) g.Count = largestGroup).OrderByDescending(Function(g) DistanceToEnd(g.Key, groupedGroups.Count)).
                    OrderByDescending(Function(g) DistanceToEnd(g.Key, revolutions.Count)).First().Key
                'Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, Middle one in middle largest group")
                mfmTrack.SelectRevolution(selectedIndex, side)
            Else
                Dim selectedIndex = validGroups.OrderByDescending(Function(g) DistanceToEnd(g.Key, groupedGroups.Count)).
                    OrderByDescending(Function(g) DistanceToEnd(g.Key, revolutions.Count)).First().Key
                'Console.WriteLine($"Track {mfmTrack.TrackNumber}, Side {side} - Selected revolution {selectedIndex}, Middle one in middle largest valid group")
                mfmTrack.SelectRevolution(selectedIndex, side)
            End If
        End If

        Return True
    End Function

    Private Function DistanceToEnd(i As Integer, count As Integer)
        Return Math.Min(i, count - i - 1)
    End Function

    Private Function NormalizeTiming(timeEntries As List(Of UShort)) As List(Of Long)
        Dim timing = New List(Of Long)

        Dim accumulatedTicks As Long = 0
        For Each entry In timeEntries
            If entry = 0 Then
                accumulatedTicks += 65536
                Continue For
            End If
            accumulatedTicks += entry

            timing.Add(accumulatedTicks)
            accumulatedTicks = 0
        Next

        Return timing
    End Function
End Class
