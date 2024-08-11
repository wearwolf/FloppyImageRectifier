Public Class ImgEncoder

    Private m_imgFile As ImgFile
    Private m_mfmImage As MfmImage

    Public Sub New(imgFile As ImgFile, mfmImage As MfmImage)
        m_imgFile = imgFile
        m_mfmImage = mfmImage
    End Sub

    Public Sub Encode()
        Dim maxTrack = 79
        If m_mfmImage.DiskType = FloppyDiskType.PC_MFM_525_360 Then
            maxTrack = 39
        End If

        Dim sectorNumber = 0
        For Each track In m_mfmImage.Tracks
            If track.TrackNumber > maxTrack Then
                Exit For
            End If

            Dim side0 = track.Side0Revolution
            If side0 IsNot Nothing Then
                EncodeSide(side0, sectorNumber)
            End If

            Dim side1 = track.Side1Revolution
            If side1 IsNot Nothing Then
                EncodeSide(side1, sectorNumber)
            End If
        Next
    End Sub

    Private Sub EncodeSide(side0 As MfmTrackRevolution, ByRef sectorNumber As Integer)
        For Each sector In side0.Sectors.OrderBy(Function(s) s.SectorNumber)
            Dim imgSector = New ImgSector(sectorNumber)
            imgSector.Data = sector.Data
            m_imgFile.Sectors.Add(imgSector)
            sectorNumber += 1
        Next
    End Sub
End Class
