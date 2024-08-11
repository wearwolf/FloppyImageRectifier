Public Class HfeEncoder

    Private Const PC_MFM_525_360_RPM = 300
    Private Const PC_MFM_525_360_BITRATE = 250
    Private Const PC_MFM_525_1200_RPM = 360
    Private Const PC_MFM_525_1200_BITRATE = 500
    Private Const PC_MFM_35_720_RPM = 300
    Private Const PC_MFM_35_720_BITRATE = 250
    Private Const PC_MFM_35_1440_RPM = 300
    Private Const PC_MFM_35_1440_BITRATE = 500

    Private m_hfeFile As HfeFile
    Private m_mfmImage As MfmImage
    Private m_wrappedSectorsFound As Boolean

    Public Sub New(hfeFile As HfeFile, mfmImage As MfmImage)
        m_hfeFile = hfeFile
        m_mfmImage = mfmImage
    End Sub

    Public Sub Encode(outputWriter As OutputWriter, rotationFixup As Boolean)
        Dim header = CreateHeader()
        m_hfeFile.Header = header

        Dim filePosition = 1

        Dim lutBlocks = CInt(Math.Ceiling(header.NumberOfTracks * 4 / HfeFile.BLOCK_LENGTH))
        filePosition += lutBlocks

        For Each track In m_mfmImage.Tracks
            Dim side0TrackData = track.Side0Revolution?.TrackData
            Dim side1TrackData = track.Side1Revolution?.TrackData

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

            Dim side0Data = If(side0TrackData IsNot Nothing, AdjustAndWrapTrackData(side0TrackData, track.Side0Revolution.Sectors, outputWriter, rotationFixup), CreateEmptyTrackData(byteCount))
            Dim side1Data = If(side1TrackData IsNot Nothing, AdjustAndWrapTrackData(side1TrackData, track.Side1Revolution.Sectors, outputWriter, rotationFixup), CreateEmptyTrackData(byteCount))

            Dim trackData = New HfeTrackData(trackOffset) With {
                .Side0 = side0Data,
                .Side1 = side1Data
            }
            m_hfeFile.Tracks.Add(trackData)
        Next

        If m_wrappedSectorsFound Then
            outputWriter.WriteLine("Tracks contain wrapping sectors")
        End If

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
                header.FloppyRpm = PC_MFM_525_360_RPM
                header.BitRate = PC_MFM_525_360_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.GENERIC_SHUGGART_DD_FLOPPYMODE
            Case FloppyDiskType.PC_MFM_525_1200
                header.BitRate = PC_MFM_525_1200_RPM
                header.BitRate = PC_MFM_525_1200_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.GENERIC_SHUGGART_DD_FLOPPYMODE
            Case FloppyDiskType.PC_MFM_35_720
                header.FloppyRpm = PC_MFM_35_720_RPM
                header.BitRate = PC_MFM_35_720_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.IBMPC_DD_FLOPPYMODE
            Case FloppyDiskType.PC_MFM_35_1440
                header.FloppyRpm = PC_MFM_35_1440_RPM
                header.BitRate = PC_MFM_35_1440_BITRATE
                header.FloppyInterfaceMode = HfeFloppyInterfaceMode.IBMPC_HD_FLOPPYMODE
        End Select

        Return header
    End Function

    Private Function CreateEmptyTrackData(length As Integer) As List(Of Byte)
        Dim totalByteCount = length + (HfeFile.BLOCK_LENGTH - length Mod (HfeFile.BLOCK_LENGTH / 2))

        Dim bytes = New List(Of Byte)
        For i = 0 To totalByteCount - 1
            bytes.Add(0)
        Next

        Return bytes
    End Function

    Private Function AdjustAndWrapTrackData(trackData As BitList, sectors As List(Of MfmSector), outputWriter As OutputWriter, rotationFixup As Boolean) As List(Of Byte)
        Dim modifiedTrack = New BitList(trackData.Data, trackData.BitCount)

        Dim wrappedSector = sectors.Where(Function(s) s.IdentStartBitIndex > s.IdentEndBitIndex OrElse s.DataStartBitIndex > s.DataEndBitIndex).FirstOrDefault
        If wrappedSector IsNot Nothing Then
            If rotationFixup Then
                Dim previousSector = sectors.Where(Function(s) s.DataEndBitIndex < wrappedSector.IdentStartBitIndex).OrderByDescending(Function(s) s.DataEndBitIndex).First()
                Dim nextSector = sectors.Where(Function(s) s.IdentStartBitIndex > wrappedSector.DataEndBitIndex).OrderBy(Function(s) s.IdentStartBitIndex).First()

                Dim distanceToPreviousGap = modifiedTrack.BitCount - wrappedSector.IdentStartBitIndex + (wrappedSector.IdentStartBitIndex - previousSector.DataEndBitIndex) \ 2
                Dim distanceToNextGap = wrappedSector.DataEndBitIndex + (nextSector.IdentStartBitIndex - wrappedSector.DataEndBitIndex) \ 2

                If distanceToPreviousGap < distanceToNextGap Then
                    outputWriter.WriteLine($"Track Number {sectors.First().TrackNumber}, Side {sectors.First().SideNumber} - Rotate right by {distanceToPreviousGap}")
                    modifiedTrack.RotateRight(distanceToPreviousGap)
                Else
                    outputWriter.WriteLine($"Track Number {sectors.First().TrackNumber}, Side {sectors.First().SideNumber} - Rotate left by {distanceToNextGap}")
                    modifiedTrack.RotateLeft(distanceToNextGap)
                End If
            Else
                m_wrappedSectorsFound = True
            End If
        End If

        Dim remainingBits = (HfeFile.BLOCK_LENGTH * 8 / 2) - (modifiedTrack.BitCount Mod (HfeFile.BLOCK_LENGTH * 8 / 2))

        Dim value = False
        For i = 0 To remainingBits - 1
            modifiedTrack.AddBit(value)
            value = Not value
        Next

        Dim reversedData = ByteBitReverser.ReverseBytes(modifiedTrack.Data)
        Return reversedData
    End Function
End Class
