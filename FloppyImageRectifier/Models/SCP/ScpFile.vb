Imports System.IO
Imports System.Text

Public Class ScpFile
#Region "Constants"

    Public Const MAX_TRACK_NUMBER = 167

#End Region

#Region "Properties"

    Public Property FilePath As String
    Public Property Header As ScpHeader
    Public Property TrackOffsets As List(Of ScpTrackOffset)
    Public Property Tracks As List(Of ScpTrack)
    Public Property Footer As ScpFooter

#End Region

#Region "Constructor"

    Public Sub New(filePath As String)
        Me.FilePath = filePath
        TrackOffsets = New List(Of ScpTrackOffset)
        Tracks = New List(Of ScpTrack)
    End Sub

#End Region

#Region "Public Methods"

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

    Public Function NeedsUpdate(diskType As FloppyDiskType) As Boolean
        If Header.DiskType <> GetScpDiskType(diskType) Then
            Return True
        End If

        Dim tip96Flag = ShouldHaveTpi96Flag(diskType)
        If tip96Flag AndAlso (Header.Flags And ScpImageFlags.Tpi96) = 0 Then
            Return True
        ElseIf Not tip96Flag AndAlso (Header.Flags And ScpImageFlags.Tpi96) <> 0 Then
            Return True
        End If

        Dim rpm360Flag = ShouldHaveRpm360Flag(diskType)
        If rpm360Flag AndAlso (Header.Flags And ScpImageFlags.Rpm360) = 0 Then
            Return True
        ElseIf Not rpm360Flag AndAlso (Header.Flags And ScpImageFlags.Rpm360) <> 0 Then
            Return True
        End If

        Return False
    End Function

    Public Sub UpdateDiskType(diskType As FloppyDiskType)
        Header.DiskType = GetScpDiskType(diskType)

        If ShouldHaveTpi96Flag(diskType) Then
            Header.Flags = Header.Flags Or ScpImageFlags.Tpi96
        Else
            Header.Flags = Header.Flags And Not ScpImageFlags.Tpi96
        End If

        If ShouldHaveRpm360Flag(diskType) Then
            Header.Flags = Header.Flags Or ScpImageFlags.Rpm360
        Else
            Header.Flags = Header.Flags And Not ScpImageFlags.Rpm360
        End If

        Dim currentTimeStamp = Footer.ImageModificationTimestamp
        Dim checksum = Header.Checksum
        For Each b In BitConverter.GetBytes(currentTimeStamp)
            checksum -= checksum
        Next

        Dim newTimeStamp = CULng(Math.Round((Date.Now - Date.UnixEpoch).TotalSeconds))
        For Each b In BitConverter.GetBytes(newTimeStamp)
            checksum += checksum
        Next

        Footer.ImageModificationTimestamp = newTimeStamp
    End Sub

#End Region

#Region "Private Methods"

    Private Function GetScpDiskType(diskType As FloppyDiskType) As ScpDiskType
        Dim scpType = ScpDiskType.Unknown
        Select Case (diskType)
            Case FloppyDiskType.PC_MFM_525_360
                scpType = ScpDiskType.disk_PC360K
            Case FloppyDiskType.PC_MFM_525_1200
                scpType = ScpDiskType.disk_PC12M
            Case FloppyDiskType.PC_MFM_35_720
                scpType = ScpDiskType.disk_PC720K
            Case FloppyDiskType.PC_MFM_35_1440
                scpType = ScpDiskType.disk_PC144M
        End Select

        Return scpType
    End Function

    Private Function ShouldHaveTpi96Flag(diskType As FloppyDiskType) As Boolean
        Return diskType = FloppyDiskType.PC_MFM_525_1200 OrElse
            diskType = FloppyDiskType.PC_MFM_35_720 OrElse
            diskType = FloppyDiskType.PC_MFM_35_1440
    End Function

    Private Function ShouldHaveRpm360Flag(diskType As FloppyDiskType) As Boolean
        Return diskType = FloppyDiskType.PC_MFM_525_1200
    End Function

#End Region

End Class
