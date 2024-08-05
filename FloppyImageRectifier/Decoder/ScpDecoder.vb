Public Class ScpDecoder
    Private m_scpFile As ScpFile

    Public Sub New(scpFile As ScpFile)
        m_scpFile = scpFile
    End Sub

    Public Function DecodeMfm(diskType As FloppyDiskType) As MfmImage
        Dim mfmImage = New MfmImage(diskType)

        Dim maxTracks = (ScpFile.MAX_TRACK_NUMBER + 1) \ 2
        For trackNumber = 0 To maxTracks
            Dim side0TrackNumber = trackNumber * 2
            Dim side1TrackNumber = trackNumber * 2 + 1

            Dim mfmTrack = New MfmTrack(trackNumber)
            Dim foundSide0Track = ProcessTrack(side0TrackNumber, diskType, mfmTrack, 0)
            Dim foundSide1Track = ProcessTrack(side1TrackNumber, diskType, mfmTrack, 1)

            If foundSide0Track OrElse foundSide1Track Then
                mfmImage.Tracks.Add(mfmTrack)
            End If
        Next

        Return mfmImage
    End Function

    Private Function ProcessTrack(scpTrackNumber As Integer, diskType As FloppyDiskType, mfmTrack As MfmTrack, side As Integer) As Boolean
        Dim scpTrack = m_scpFile.Tracks.FirstOrDefault(Function(t) t.TrackOffset.TrackNumber = scpTrackNumber)
        If scpTrack Is Nothing Then
            Return False
        End If

        Dim fluxDecoder = New MfmFluxDecoder(diskType, m_scpFile.Header.CaptureResolutionInSeconds)
        For Each revolution In scpTrack.TrackRevolutionData
            Dim timings = NormalizeTiming(revolution.TimeEntries)

            Dim revolutionBitList = fluxDecoder.Decode(timings)
            Dim decoder = New MfmTrackRevolutionDecoder(revolutionBitList)
            Dim decodedRevolution = decoder.Decode()
            mfmTrack.AddRevolution(decodedRevolution, side)
        Next

        Dim revolutions = mfmTrack.GetRevolutions(side)
        Dim revGroups = New Dictionary(Of Integer, Integer)(revolutions.Count)

        For i = 0 To revolutions.Count - 1
            revGroups(i) = i + 1
        Next

        For i = 1 To revolutions.Count
            Dim index = revGroups.FirstOrDefault(Function(g) g.Value = i)
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
                mfmTrack.SelectRevolution(selectedIndex, side)
            Else
                Dim validRevolutions = group.Where(Function(r) revolutions(r.Key).IsValid)
                If validRevolutions.Count = 1 Then
                    Dim selectedIndex = validRevolutions(0).Key
                    mfmTrack.SelectRevolution(selectedIndex, side)
                Else
                    Dim selectedIndex = validRevolutions.Min(Function(g) g.Key)
                    mfmTrack.SelectRevolution(selectedIndex, side)
                End If
            End If
        Else
            Dim validGroups = groupedGroups.Where(Function(g) g.Count = largestGroup AndAlso revolutions(g.First().Key).IsValid)
            If validGroups.Count = 1 Then
                Dim selectedIndex = validGroups(0).Min(Function(g) g.Key)
                mfmTrack.SelectRevolution(selectedIndex, side)
            Else
                Dim selectedIndex = validGroups.Last().Min(Function(g) g.Key)
                mfmTrack.SelectRevolution(selectedIndex, side)
            End If
        End If

        Return True
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
