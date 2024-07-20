Imports System.IO

Public Class ScpFooter
    Private Const FOOTER_LENGTH = 48
    Private Const SIGNATURE_LENGTH = 4

    Public Property DriveManufacturerStringOffset As UInteger
    Public Property DriveModelStringOffset As UInteger
    Public Property DriveSerialNumberStringOffset As UInteger
    Public Property CreatorStringOffset As UInteger
    Public Property ApplicationNameStringOffset As UInteger
    Public Property CommentsStringOffset As UInteger
    Public Property ImageCreationTimestamp As ULong
    Public Property ImageModificationTimestamp As ULong
    Public Property ApplicationVersion As Byte
    Public Property ScpHardwareVersion As Byte
    Public Property ScpFirmwareVersion As Byte
    Public Property ImageFormatRevision As Byte
    Public Property Signature As String

    Public Property DriveManufacturerString As String
    Public Property DriveModelString As String
    Public Property DriveSerialNumberString As String
    Public Property CreatorString As String
    Public Property ApplicationNameString As String
    Public Property CommentsString As String

    Public Sub Read(fstream As FileStream, binReader As BinaryReader)
        fstream.Seek(-1 * FOOTER_LENGTH, SeekOrigin.End)
        DriveManufacturerStringOffset = binReader.ReadUInt32()
        DriveModelStringOffset = binReader.ReadUInt32()
        DriveSerialNumberStringOffset = binReader.ReadUInt32()
        CreatorStringOffset = binReader.ReadUInt32()
        ApplicationNameStringOffset = binReader.ReadUInt32()
        CommentsStringOffset = binReader.ReadUInt32()
        ImageCreationTimestamp = binReader.ReadInt64()
        ImageModificationTimestamp = binReader.ReadInt64()
        ApplicationVersion = binReader.ReadByte()
        ScpHardwareVersion = binReader.ReadByte()
        ScpFirmwareVersion = binReader.ReadByte()
        ImageFormatRevision = binReader.ReadByte()
        Signature = binReader.ReadChars(SIGNATURE_LENGTH)

        If DriveManufacturerStringOffset <> 0 Then
            fstream.Seek(DriveManufacturerStringOffset, SeekOrigin.Begin)
            DriveManufacturerString = GetString(binReader)
        End If

        If DriveModelStringOffset <> 0 Then
            fstream.Seek(DriveModelStringOffset, SeekOrigin.Begin)
            DriveModelString = GetString(binReader)
        End If

        If DriveSerialNumberStringOffset <> 0 Then
            fstream.Seek(DriveSerialNumberStringOffset, SeekOrigin.Begin)
            DriveSerialNumberString = GetString(binReader)
        End If

        If CreatorStringOffset <> 0 Then
            fstream.Seek(CreatorStringOffset, SeekOrigin.Begin)
            CreatorString = GetString(binReader)
        End If

        If ApplicationNameStringOffset <> 0 Then
            fstream.Seek(ApplicationNameStringOffset, SeekOrigin.Begin)
            ApplicationNameString = GetString(binReader)
        End If

        If CommentsStringOffset <> 0 Then
            fstream.Seek(CommentsStringOffset, SeekOrigin.Begin)
            CommentsString = GetString(binReader)
        End If
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"DRIVE MANUFACTURER STRING = '{DriveManufacturerString}'")
        outputWriter.WriteLine($"DRIVE MODEL STRING = '{DriveModelString}'")
        outputWriter.WriteLine($"DRIVE SERIAL NUMBER STRING = '{DriveSerialNumberString}'")
        outputWriter.WriteLine($"CREATOR STRING = '{CreatorString}'")
        outputWriter.WriteLine($"APPLICATION NAME STRING = '{ApplicationNameString}'")
        outputWriter.WriteLine($"COMMENTS STRING = '{CommentsString}'")
        outputWriter.WriteLine($"IMAGE CREATION TIMESTAMP = {Date.UnixEpoch.AddSeconds(ImageCreationTimestamp)}")
        outputWriter.WriteLine($"IMAGE MODIFICATION TIMESTAMP = {Date.UnixEpoch.AddSeconds(ImageModificationTimestamp)}")
        outputWriter.WriteLine($"APPLICATION VERSION = {ApplicationVersion:X2}")
        outputWriter.WriteLine($"SCP HARDWARE VERSION = {ScpHardwareVersion:X2}")
        outputWriter.WriteLine($"SCP FIRMWARE VERSION = {ScpFirmwareVersion:X2}")
        outputWriter.WriteLine($"IMAGE FORMAT REVISION = {ImageFormatRevision:X2}")
        outputWriter.WriteLine($"SIGNATURE = {Signature}")
    End Sub

    Private Function GetString(binReader As BinaryReader) As String
        Dim length = binReader.ReadUInt16()
        Dim str = binReader.ReadChars(length)
        Return str
    End Function
End Class
