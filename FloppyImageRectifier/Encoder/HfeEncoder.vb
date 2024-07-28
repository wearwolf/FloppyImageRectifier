Public Class HfeEncoder

    Private Const IBM_MFM_525_360_RPM = 300
    Private Const IBM_MFM_525_360_BITRATE = 250
    Private Const IBM_MFM_525_1200_RPM = 360
    Private Const IBM_MFM_525_1200_BITRATE = 500
    Private Const IBM_MFM_35_720_RPM = 300
    Private Const IBM_MFM_35_720_BITRATE = 250
    Private Const IBM_MFM_35_1440_RPM = 300
    Private Const IBM_MFM_35_1440_BITRATE = 500

    Private m_hfeFile As HfeFile
    Private m_mfmImage As MfmImage

    Public Sub New(hfeFile As HfeFile, mfmImage As MfmImage)
        m_hfeFile = hfeFile
        m_mfmImage = mfmImage
    End Sub

    Public Sub Encode()
        Dim header = CreateHeader()
        m_hfeFile.Header = header

        Dim filePosition = 1

        Dim lutBlocks = CInt(Math.Ceiling(header.NumberOfTracks * 4 / HfeFile.BLOCK_LENGTH))
        filePosition += lutBlocks

        For Each track In m_mfmImage.Tracks
            Dim side0TrackData = track.Side0Revolution.TrackData
            Dim side1TrackData = track.Side1Revolution.TrackData

            Dim maxBitCount = Math.Max(side0TrackData.BitCount, side1TrackData.BitCount)
            Dim byteCount = CInt(Math.Ceiling(maxBitCount / 8))
            Dim trackLength = byteCount * 2
            Dim blockCount = CInt(Math.Ceiling(trackLength / HfeFile.BLOCK_LENGTH))

            Dim trackOffset = New HfeTrackOffset() With {
                .TrackNumber = track.TrackNumber,
                .Offset = filePosition,
                .TrackLength = trackLength
            }
            m_hfeFile.TrackOffsets.Add(trackOffset)

            filePosition += blockCount

            Dim side0Data = WrapTrackData(side0TrackData)
            Dim side1Data = WrapTrackData(side1TrackData)

            Dim trackData = New HfeTrackData(trackOffset) With {
                .Side0 = side0Data,
                .Side1 = side1Data
            }
            m_hfeFile.Tracks.Add(trackData)
        Next

    End Sub

    Private Function CreateHeader() As HfeHeader
        Dim header = New HfeHeader With {
                    .Signature = HfeHeader.EXPECTED_SIGNATURE,
                    .FormatVersion = 0,
                    .NumberOfTracks = m_mfmImage.Tracks.Count,
                    .NumberOfSides = 2,
                    .TrackEncoding = HfeTrackEncoding.ISOIBM_MFM_ENCODING,
                    .Dnu = &HFF,
                    .TrackListOffset = 1,
                    .WriteAllowed = &HFF,
                    .SingleStep = &HFF,
                    .Track0s0AltEncoding = &HFF,
                    .Track0s0Encoding = HfeTrackEncoding.UNKNOWN_ENCODING,
                    .Track0s1AltEncoding = &HFF,
                    .Track0s1Encoding = HfeTrackEncoding.UNKNOWN_ENCODING
                }

        Select Case (m_mfmImage.DiskType)
            Case FloppyDiskType.PC_MFM_525_360
                header.FloppyRpm = IBM_MFM_525_360_RPM
                header.BitRate = IBM_MFM_525_360_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.GENERIC_SHUGGART_DD_FLOPPYMODE
            Case FloppyDiskType.PC_MFM_525_1200
                header.BitRate = IBM_MFM_525_1200_RPM
                header.BitRate = IBM_MFM_525_1200_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.GENERIC_SHUGGART_DD_FLOPPYMODE
            Case FloppyDiskType.PC_MFM_35_720
                header.FloppyRpm = IBM_MFM_35_720_RPM
                header.BitRate = IBM_MFM_35_720_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.IBMPC_DD_FLOPPYMODE
            Case FloppyDiskType.PC_MFM_35_1440
                header.FloppyRpm = IBM_MFM_35_1440_RPM
                header.BitRate = IBM_MFM_35_1440_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.IBMPC_HD_FLOPPYMODE
        End Select

        Return header
    End Function

    Private Function WrapTrackData(trackData As BitList) As List(Of Byte)
        Dim remainingBits = trackData.BitCount Mod (HfeFile.BLOCK_LENGTH * 8 / 2)

        trackData.ResetRead()
        For i = 0 To remainingBits
            Dim bit = trackData.ReadBit()
            trackData.AddBit(bit)
        Next

        Dim reversedData = ByteBitReverser.ReverseBytes(trackData.Data)
        Return reversedData
    End Function
End Class
