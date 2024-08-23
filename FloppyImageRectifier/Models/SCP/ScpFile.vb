Imports System.IO
Imports System.Text

Public Class ScpFile
    Public Const MAX_TRACK_NUMBER = 167

    Public Property FilePath As String
    Public Property Header As ScpHeader
    Public Property TrackOffsets As List(Of ScpTrackOffset)
    Public Property Tracks As List(Of ScpTrack)
    Public Property Footer As ScpFooter

    Public Sub New(filePath As String)
        Me.FilePath = filePath
        TrackOffsets = New List(Of ScpTrackOffset)
        Tracks = New List(Of ScpTrack)
    End Sub

    Public Sub Read()
        Using fstream = File.OpenRead(FilePath)
            Using binReader = New BinaryReader(fstream, Encoding.Latin1)
                Header = New ScpHeader()
                Header.Read(binReader)

                For index = 0 To MAX_TRACK_NUMBER
                    Dim trackDataHeaderOffset = New ScpTrackOffset()
                    trackDataHeaderOffset.Read(binReader, index)

                    TrackOffsets.Add(trackDataHeaderOffset)
                Next

                For Each trackOffset In TrackOffsets
                    If trackOffset.TrackNumber < Header.StartTrack OrElse
                            trackOffset.TrackNumber > Header.EndTrack OrElse
                            trackOffset.Offset = 0 Then
                        Continue For
                    End If

                    Dim track = New ScpTrack(trackOffset)
                    track.Read(fstream, binReader, Header.NumberOfRevolutionsPerTrack)

                    Tracks.Add(track)
                Next

                Footer = New ScpFooter()
                Footer.Read(fstream, binReader)
            End Using
        End Using
    End Sub

    Public Sub Write()
        Using fstream = File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite)
            Using BinaryWriter = New BinaryWriter(fstream, Encoding.Latin1)
                ' Only the header and footer are written because those are the only parts that should be changed
                Header.Write(BinaryWriter)

                Footer.Write(fstream, BinaryWriter)
            End Using
        End Using
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        Header.WriteOutput(outputWriter)
        outputWriter.WriteLine()

        For Each trackOffset In TrackOffsets
            trackOffset.WriteOutput(outputWriter)
            outputWriter.WriteLine()
        Next

        For Each track In Tracks
            track.WriteOutput(outputWriter)
        Next

        Footer.WriteOutput(outputWriter)
    End Sub

    Friend Sub UpdateDiskType(diskType As FloppyDiskType)
        If (Header.DiskType = ScpDiskType.disk_360) Then ' Default for GW
            Select Case (diskType)
                Case FloppyDiskType.PC_MFM_525_360
                    Header.DiskType = ScpDiskType.disk_PC360K
                Case FloppyDiskType.PC_MFM_525_1200
                    Header.DiskType = ScpDiskType.disk_PC12M
                Case FloppyDiskType.PC_MFM_35_720
                    Header.DiskType = ScpDiskType.disk_PC720K
                Case FloppyDiskType.PC_MFM_35_1440
                    Header.DiskType = ScpDiskType.disk_PC144M
            End Select
        End If

        Select Case (diskType)
            Case FloppyDiskType.PC_MFM_525_360
                Header.Flags = Header.Flags And Not ScpImageFlags.Tpi96
                Header.Flags = Header.Flags And Not ScpImageFlags.Rpm360
            Case FloppyDiskType.PC_MFM_525_1200
                Header.Flags = Header.Flags Or ScpImageFlags.Tpi96
                Header.Flags = Header.Flags Or ScpImageFlags.Rpm360
            Case FloppyDiskType.PC_MFM_35_720
                Header.Flags = Header.Flags Or ScpImageFlags.Tpi96
                Header.Flags = Header.Flags And Not ScpImageFlags.Rpm360
            Case FloppyDiskType.PC_MFM_35_1440
                Header.Flags = Header.Flags Or ScpImageFlags.Tpi96
                Header.Flags = Header.Flags And Not ScpImageFlags.Rpm360
        End Select

        Dim currentTimeStamp = Footer.ImageModificationTimestamp

        Dim newTimeStamp = CULng(Math.Round((Date.Now - Date.UnixEpoch).TotalSeconds))

        Dim checksum = Header.Checksum
        For Each b In BitConverter.GetBytes(currentTimeStamp)
            checksum -= checksum
        Next

        For Each b In BitConverter.GetBytes(newTimeStamp)
            checksum += checksum
        Next

        Footer.ImageModificationTimestamp = newTimeStamp
    End Sub
End Class
