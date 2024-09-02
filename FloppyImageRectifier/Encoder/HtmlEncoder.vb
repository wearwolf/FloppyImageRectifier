Imports System.IO
Imports System.Text
Imports System.Web

Public Class HtmlEncoder

#Region "Constants"

    Private Const HEAD_START As String = "<!DOCTYPE html>
<html lang=""en-CA"">
<head>"
    Private Const TITLE_FORMAT As String = "<title>{0}: Track: {1}, Side {2}</title>"
    Private Const STYLE As String = " <style type=""text/css"">
        .unknown {
            color:#898989;
            font-size: 20px;
        }

        .clock {
            color:#898989;
            font-size: 20px;
        }

        .data {
            color: black;
            font-size: 20px;
            font-weight: bold;
        }

        .bitBlock {
            display: inline-block;
            border-top: 1px solid black;
            border-bottom: 1px solid black;
            border-left: 1px solid transparent;
            border-right: 1px solid transparent;
            width: 18px;
            height: 23px;
            text-align:center;
        }

        .groupStart {
            border-left: 1px solid black;
        }

        .groupEnd {
            border-right: 1px solid black;
        }

        .track {
            outline: 1px solid black;
            font-size: 0px;
            width: 1600px
        }

        .gap {
            background-color: #e0e0e0;
        }

        .identMark {
            background-color: #b9e6fc;
        }

        .trackNumber {
            background-color: #4cc3fc;
        }

        .sideNumber {
            background-color: #5fe7fa;
        }

        .sectorNumber {
            background-color: #5db2fb;
        }

        .sectorSize {
            background-color: #5f88fa;
        }

        .checksum {
            background-color: #b730ff;
        }

        .dataBlock {
            background-color: #fa85ff;
        }
    </style>"
    Private Const HEAD_END As String = "</head>"
    Private Const BODY_START As String = "<body>"
    Private Const HEADER_FORMAT As String = "<h1>{0}: Track: {1}, Side: {2}</h1>"
    Private Const HEADER_REVOLUTION_FORMAT As String = "<h1>{0}: Track: {1}, Side: {2}, Revolution: {3}</h1>"
    Private Const TRACK_START As String = "<div class=""track"">"
    Private Const BIT_CLASS_UNKNOWN As String = "unknown"
    Private Const BIT_CLASS_CLOCK As String = "clock"
    Private Const BIT_CLASS_DATA As String = "data"
    Private Const BIT_FORMAT As String = "<span class=""{0}"">
                {1}
            </span>"
    Private Const BIT_TIME_FORMAT As String = "<span class=""{0}"" title=""{2} microseconds"">
                {1}
            </span>"

    Private Const BIT_BLOCK_START_TEXT_FORMAT As String = "<span class=""bitBlock {0}"" title=""{1}"">"
    Private Const BIT_BLOCK_START_HEX_FORMAT As String = "<span class=""bitBlock {0}"" title=""{1} - 0x{2:X2}"">"
    Private Const BIT_BLOCK_START_HEX_BRACKET_FORMAT As String = "<span class=""bitBlock {0}"" title=""{1} - 0x{2:X2} ({2})"">"
    Private Const BIT_BLOCK_START_HEX_BRACKET_TEXT_FORMAT As String = "<span class=""bitBlock {0}"" title=""{1} - 0x{2:X2} ({3})"">"
    Private Const BIT_BLOCK_START_HEX_WORD_FORMAT As String = "<span class=""bitBlock {0}"" title=""{1} - 0x{2:X4}"">"

    Private Const GROUP_START_CLASS As String = " groupStart" ' starting space for concatenation
    Private Const GROUP_END_CLASS As String = " groupEnd" ' starting space for concatenation

    Private Const GAP_CLASS As String = "gap"
    Private Const IDENTIFIER_CLASS As String = "identMark"
    Private Const TRACK_NUMBER_CLASS = "trackNumber"
    Private Const SIDE_NUMBER_CLASS = "sideNumber"
    Private Const SECTOR_NUMBER_CLASS = "sectorNumber"
    Private Const SECTOR_SIZE_CLASS = "sectorSize"
    Private Const CHECKSUM_CLASS = "checksum"
    Private Const DATA_BLOCK_CLASS = "dataBlock"

    Private Const SPAN_END As String = "</span>"
    Private Const DIV_END As String = "</div>"
    Private Const BODY_END As String = "</body>
</html>"

    Private Const IDENTIFIER_SIGNATURE_BYTE As Byte = &HA1
    Private Const IDENTIFIER_SECTOR_MARK_BYTE As Byte = &HFE
    Private Const IDENTIIFER_DATA_MARK_BYTE As Byte = &HFB

#End Region

#Region "Enums"

    Private Enum ByteFormat
        HexByte
        HexByteAndBracketDecimal
        HexByteAndBracketText
        HexWord
    End Enum

#End Region

#Region "Fields"

    Private m_mfmImage As MfmImage

    Private m_htmlFolderPath As String
    Private m_htmlBaseFileName As String

#End Region

#Region "Constructor"

    Public Sub New(mfmImage As MfmImage, htmlPath As String)
        m_mfmImage = mfmImage

        m_htmlFolderPath = htmlPath
        m_htmlBaseFileName = Path.GetFileNameWithoutExtension(htmlPath)
    End Sub

#End Region

#Region "Public Methods"

    Public Sub GenerateFiles(outputWriter As OutputWriter)
        If Not Directory.Exists(m_htmlFolderPath) Then
            Directory.CreateDirectory(m_htmlFolderPath)
        End If

        For Each track In m_mfmImage.Tracks
            If track.Side0Revolution IsNot Nothing Then
                GenerateTrackFile(track.Side0Revolution, track.TrackNumber, 0, track.Side0Revolutions.Count > 1, outputWriter)
            End If

            If track.Side1Revolution IsNot Nothing Then
                GenerateTrackFile(track.Side1Revolution, track.TrackNumber, 1, track.Side1Revolutions.Count > 1, outputWriter)
            End If
        Next
    End Sub

#End Region

#Region "Private Methods"

    Private Sub GenerateTrackFile(revolution As MfmTrackRevolution, trackNumber As Integer, sideNumber As Integer, includeRevolutionNumber As Boolean, outputWriter As OutputWriter)
        Dim fileName = m_htmlBaseFileName & $"_t{trackNumber}_s{sideNumber}.html"

        outputWriter.WriteLine($"Writing file: {fileName}")

        Dim sb = New StringBuilder

        sb.AppendLine(HEAD_START)
        sb.AppendFormat(TITLE_FORMAT, m_mfmImage.FileName, trackNumber, sideNumber)
        sb.AppendLine()
        sb.AppendLine(STYLE)
        sb.AppendLine(HEAD_END)
        sb.AppendLine(BODY_START)

        If includeRevolutionNumber Then
            sb.AppendFormat(HEADER_REVOLUTION_FORMAT, m_mfmImage.FileName, trackNumber, sideNumber, revolution.RevolutionNumber)
        Else
            sb.AppendFormat(HEADER_FORMAT, m_mfmImage.FileName, trackNumber, sideNumber)
        End If

        sb.AppendLine()
        sb.AppendLine(TRACK_START)

        FormatTrack(revolution, sb)

        sb.AppendLine(DIV_END)
        sb.AppendLine(BODY_END)

        Using writer = New StreamWriter(Path.Join(m_htmlFolderPath, fileName), append:=False, Encoding.Latin1)
            writer.Write(sb)
        End Using
    End Sub

    Private Sub FormatTrack(revolution As MfmTrackRevolution, sb As StringBuilder)
        Dim bitList = revolution.TrackData
        Dim timeEntries = revolution.TimeEntries?.GetEnumerator()
        bitList.ResetRead()

        If revolution.Sectors.Any() Then
            Dim gapName = "Index Gap"

            For Each sector In revolution.Sectors
                FormatGap(bitList, timeEntries, sb, gapName, sector.IdentStartBitIndex)

                FormatSector(bitList, timeEntries, sector, sb)

                gapName = "Data Gap"
            Next

            FormatGap(bitList, timeEntries, sb, gapName, Long.MaxValue)
        Else
            FormatGap(bitList, timeEntries, sb, "Empty Track", Long.MaxValue)
        End If
    End Sub

    Private Shared Sub FormatGap(bitList As BitList, timeEntries As IEnumerator(Of Double), sb As StringBuilder, title As String, stopPosition As Long)
        Dim bitBlockStart = String.Format(BIT_BLOCK_START_TEXT_FORMAT, GAP_CLASS & GROUP_START_CLASS, title)
        Dim bitBlock = String.Format(BIT_BLOCK_START_TEXT_FORMAT, GAP_CLASS, title)
        Dim bitBlockEnd = String.Format(BIT_BLOCK_START_TEXT_FORMAT, GAP_CLASS & GROUP_END_CLASS, title)

        Dim count = 0
        Dim maxCount = stopPosition - bitList.ReadPosition
        While Not bitList.IsEof AndAlso count < maxCount
            If count = 0 Then
                sb.AppendLine(bitBlockStart)
            ElseIf count = (maxCount - 1) Then
                sb.AppendLine(bitBlockEnd)
            Else
                sb.AppendLine(bitBlock)
            End If

            FormatBit(bitList, timeEntries, sb, gap:=True)

            sb.AppendLine(SPAN_END)

            count += 1
        End While
    End Sub

    Private Sub FormatSector(bitList As BitList, timeEntries As IEnumerator(Of Double), sector As MfmSector, sb As StringBuilder)
        FormatIdentifierMarkl(bitList, timeEntries, sb, IDENTIFIER_SECTOR_MARK_BYTE)

        FormatByte(bitList, timeEntries, sb, TRACK_NUMBER_CLASS, "Track Number", sector.TrackNumber, ByteFormat.HexByteAndBracketDecimal)
        FormatByte(bitList, timeEntries, sb, SIDE_NUMBER_CLASS, "Side Number", sector.SideNumber, ByteFormat.HexByteAndBracketDecimal)
        FormatByte(bitList, timeEntries, sb, SECTOR_NUMBER_CLASS, "Sector Number", sector.SectorNumber, ByteFormat.HexByteAndBracketDecimal)
        FormatByte(bitList, timeEntries, sb, SECTOR_SIZE_CLASS, "Sector Size", sector.SectorSize, ByteFormat.HexByteAndBracketText, $"{sector.SectorSizeInBytes} bytes")

        FormatByte(bitList, timeEntries, sb, CHECKSUM_CLASS, "Checksum", sector.IdentChecksum, ByteFormat.HexWord)
        FormatByte(bitList, timeEntries, sb, CHECKSUM_CLASS, "Checksum", sector.IdentChecksum, ByteFormat.HexWord)

        FormatGap(bitList, timeEntries, sb, "Data Gap", sector.DataStartBitIndex)

        FormatIdentifierMarkl(bitList, timeEntries, sb, IDENTIIFER_DATA_MARK_BYTE)

        For Each b In sector.Data
            Dim c = ""
            If Not Char.IsControl(Chr(b)) Then
                c = HttpUtility.HtmlEncode(Chr(b))
            End If

            FormatByte(bitList, timeEntries, sb, DATA_BLOCK_CLASS, "Data", b, ByteFormat.HexByteAndBracketText, c)
        Next

        FormatByte(bitList, timeEntries, sb, CHECKSUM_CLASS, "Checksum", sector.DataChecksum, ByteFormat.HexWord)
        FormatByte(bitList, timeEntries, sb, CHECKSUM_CLASS, "Checksum", sector.DataChecksum, ByteFormat.HexWord)
    End Sub

    Private Sub FormatIdentifierMarkl(bitList As BitList, timeEntries As IEnumerator(Of Double), sb As StringBuilder, markByte As Byte)
        For i = 1 To 12
            FormatByte(bitList, timeEntries, sb, IDENTIFIER_CLASS, "Identifier Mark", 0, ByteFormat.HexByte)
        Next

        For i = 1 To 3
            FormatByte(bitList, timeEntries, sb, IDENTIFIER_CLASS, "Identifier Mark", IDENTIFIER_SIGNATURE_BYTE, ByteFormat.HexByte)
        Next

        FormatByte(bitList, timeEntries, sb, IDENTIFIER_CLASS, "Identifier Mark", markByte, ByteFormat.HexByte)
    End Sub

    Private Sub FormatByte(bitList As BitList, timeEntries As IEnumerator(Of Double), sb As StringBuilder, bitBlockClass As String, title As String, value As Integer, format As ByteFormat, Optional bracketText As String = "")
        Dim bitBlockStart = String.Empty
        Dim bitBlock = String.Empty
        Dim bitBlockEnd = String.Empty
        Select Case format
            Case ByteFormat.HexByte
                bitBlockStart = String.Format(BIT_BLOCK_START_HEX_FORMAT, bitBlockClass & GROUP_START_CLASS, title, value)
                bitBlock = String.Format(BIT_BLOCK_START_HEX_FORMAT, bitBlockClass, title, value)
                bitBlockEnd = String.Format(BIT_BLOCK_START_HEX_FORMAT, bitBlockClass & GROUP_END_CLASS, title, value)
            Case ByteFormat.HexByteAndBracketDecimal
                bitBlockStart = String.Format(BIT_BLOCK_START_HEX_BRACKET_FORMAT, bitBlockClass & GROUP_START_CLASS, title, value)
                bitBlock = String.Format(BIT_BLOCK_START_HEX_BRACKET_FORMAT, bitBlockClass, title, value)
                bitBlockEnd = String.Format(BIT_BLOCK_START_HEX_BRACKET_FORMAT, bitBlockClass & GROUP_END_CLASS, title, value)
            Case ByteFormat.HexByteAndBracketText
                bitBlockStart = String.Format(BIT_BLOCK_START_HEX_BRACKET_TEXT_FORMAT, bitBlockClass & GROUP_START_CLASS, title, value, bracketText)
                bitBlock = String.Format(BIT_BLOCK_START_HEX_BRACKET_TEXT_FORMAT, bitBlockClass, title, value, bracketText)
                bitBlockEnd = String.Format(BIT_BLOCK_START_HEX_BRACKET_TEXT_FORMAT, bitBlockClass & GROUP_END_CLASS, title, value, bracketText)
            Case ByteFormat.HexWord
                bitBlockStart = String.Format(BIT_BLOCK_START_HEX_WORD_FORMAT, bitBlockClass & GROUP_START_CLASS, title, value)
                bitBlock = String.Format(BIT_BLOCK_START_HEX_WORD_FORMAT, bitBlockClass, title, value)
                bitBlockEnd = String.Format(BIT_BLOCK_START_HEX_WORD_FORMAT, bitBlockClass & GROUP_END_CLASS, title, value)
        End Select

        Dim count = 0
        Dim clock = True
        While Not bitList.IsEof AndAlso count < 16
            If count = 0 Then
                sb.AppendLine(bitBlockStart)
            ElseIf count = 15 Then
                sb.AppendLine(bitBlockEnd)
            Else
                sb.AppendLine(bitBlock)
            End If

            FormatBit(bitList, timeEntries, sb, gap:=False, clock)

            sb.AppendLine(SPAN_END)

            count += 1
            clock = Not clock
        End While
    End Sub

    Private Shared Sub FormatBit(bitList As BitList, timeEntries As IEnumerator(Of Double), sb As StringBuilder, gap As Boolean, Optional clock As Boolean = False)
        Dim bit = bitList.ReadBit()

        Dim bitString = If(bit, "1", "0")

        Dim bitClass = BIT_CLASS_UNKNOWN
        If Not gap Then
            bitClass = If(clock, BIT_CLASS_CLOCK, BIT_CLASS_DATA)
        End If

        If bit AndAlso timeEntries IsNot Nothing Then
            timeEntries.MoveNext()
            Dim time = timeEntries.Current * 1000000.0
            sb.AppendFormat(BIT_TIME_FORMAT, bitClass, bitString, time)
        Else
            sb.AppendFormat(BIT_FORMAT, bitClass, bitString)
        End If

        sb.AppendLine()
    End Sub

#End Region

End Class
