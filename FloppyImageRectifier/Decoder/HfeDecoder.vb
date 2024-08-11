Public Class HfeDecoder
    Private m_hfeFile As HfeFile

    Public Sub New(hfeFile As HfeFile)
        m_hfeFile = hfeFile
    End Sub

    Public Function DecodeMfm(diskType As FloppyDiskType) As MfmImage
        Dim mfmImage = New MfmImage(diskType)

        For Each hfeTrack In m_hfeFile.Tracks
            Dim track = New MfmTrack(hfeTrack.TrackOffset.TrackNumber)
            Dim sideLength = hfeTrack.TrackOffset.TrackLength / 2
            Dim side0Revolution = DecodeSide(hfeTrack.Side0, sideLength)
            track.AddRevolution(side0Revolution, 0)

            Dim side1Revolution = DecodeSide(hfeTrack.Side1, sideLength)
            track.AddRevolution(side1Revolution, 1)

            mfmImage.AddTrack(track)
        Next

        Return mfmImage
    End Function

    Private Shared Function DecodeSide(data As List(Of Byte), length As Integer) As MfmTrackRevolution
        Dim bitList = New BitList(ByteBitReverser.ReverseBytes(data), length * 8)
        Dim decoder = New MfmTrackRevolutionDecoder(bitList)
        Dim revolution = decoder.Decode()
        Return revolution
    End Function
End Class
