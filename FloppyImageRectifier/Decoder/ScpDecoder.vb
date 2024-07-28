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
