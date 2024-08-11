Public Class MfmTrackRevolutionDecoder

#Region "Constants"

    Private Const IDENTIFIER_MARK As UShort = &H4489          ' A1 with incorrect clock value
    Private Const IDENTIFIER_MARK_SECTOR As UShort = &H5554   ' FE
    Private Const IDENTIFIER_MARK_DATA As UShort = &H5545     ' FB

    Private Const IDENTIFIER_MARK_EDC As UShort = &H443B      ' EDC after reading in A1

    Private Const SECTOR_SIZE_MULTIPLIER As Integer = 128
#End Region

#Region "Fields"

    Private m_bitList As BitList

    Private m_edc As UShort

    Private m_reachedEndOfTrack As Boolean

#End Region

#Region "Constructor"

    Public Sub New(bitList As BitList)
        m_bitList = bitList
    End Sub

#End Region

#Region "Public Methods"

    Public Function Decode() As MfmTrackRevolution
        m_bitList.ResetRead()
        Dim sectors = New List(Of MfmSector)

        m_reachedEndOfTrack = False
        While Not m_reachedEndOfTrack
            If Not FindSectorIdentifierMark() Then
                Exit While
            End If

            Dim identifierStartIndex = m_bitList.ReadPosition - 16

            Dim cylinder = ReadByte()
            Dim side = ReadByte()
            Dim sectorNumber = ReadByte()
            Dim sectorSize = ReadByte()

            Dim calculatedIdentifierEdc = m_edc
            Dim identifierEdc = ReadUShort()

            Dim identifierEndIndex = m_bitList.ReadPosition

            If Not FindDataIdentifierMark() Then
                Exit While
            End If

            Dim dataStartIndex = m_bitList.ReadPosition - 16

            Dim data = ReadBytes(SECTOR_SIZE_MULTIPLIER * Math.Pow(2, sectorSize))

            Dim calculatedDataEdc = m_edc
            Dim dataEdc = ReadUShort()

            Dim dataEndIndex = m_bitList.ReadPosition

            Dim sector = New MfmSector(identifierStartIndex,
                                       cylinder,
                                       side,
                                       sectorNumber,
                                       sectorSize,
                                       identifierEdc,
                                       calculatedIdentifierEdc,
                                       identifierEndIndex,
                                       dataStartIndex,
                                       data,
                                       dataEdc,
                                       calculatedDataEdc,
                                       dataEndIndex)
            sectors.Add(sector)
        End While

        Dim revolution = New MfmTrackRevolution(m_bitList, sectors)
        Return revolution
    End Function

#End Region

#Region "Private Methods"

    Public Function FindSectorIdentifierMark() As Boolean
        Return FindIdentifierMark(IDENTIFIER_MARK_SECTOR, shouldReset:=False)
    End Function

    Public Function FindDataIdentifierMark() As Boolean
        Return FindIdentifierMark(IDENTIFIER_MARK_DATA, shouldReset:=True)
    End Function

    Public Function ReadByte() As Byte
        Dim b As Byte = 0

        For i = 0 To 7
            ReadBit() ' ignore clock bit
            Dim bit = ReadBit()
            ShiftEdc(bit)

            b = b << 1
            If bit Then
                b += 1
            End If
        Next

        Return b
    End Function

    Public Function ReadUShort() As UShort
        Dim s As UShort = 0

        For i = 0 To 15
            ReadBit() ' ignore clock bit
            Dim bit = ReadBit()
            ShiftEdc(bit)

            s = s << 1
            If bit Then
                s += 1
            End If
        Next

        Return s
    End Function

    Public Function ReadBytes(count As Integer) As List(Of Byte)
        Dim data = New List(Of Byte)(count)

        For i = 1 To count
            Dim b = ReadByte()
            data.Add(b)
        Next

        Return data
    End Function

    Private Function ReadBit() As Boolean
        If m_bitList.IsEof Then
            m_reachedEndOfTrack = True
            m_bitList.ResetRead()
        End If

        Return m_bitList.ReadBit()
    End Function

    Private Function FindIdentifierMark(typeMark As UShort, shouldReset As Boolean) As Boolean
        Dim bytesToCheck As UShort = 0

        Dim bitsRead = 0
        While bitsRead <= m_bitList.BitCount
            If m_reachedEndOfTrack AndAlso Not shouldReset Then
                Return False
            End If

            For i = 0 To 2
                If Not FindRawValue(IDENTIFIER_MARK) Then
                    ReadBit()
                    Continue While
                End If
            Next

            If Not FindRawValue(typeMark) Then
                Continue While
            End If

            Return True
        End While

        Return False
    End Function

    Private Function FindRawValue(value As UShort) As Boolean
        m_bitList.SavePosition()
        Dim nextShort = ReadRawUShort()
        If nextShort <> value Then
            m_bitList.RestorePosition()
            ResetEdc()
            Return False
        End If
        Return True
    End Function

    Public Function ReadRawUShort() As UShort
        Dim s As UShort = 0

        For i = 0 To 15
            Dim bit = ReadBit()

            If i Mod 2 = 1 Then
                ShiftEdc(bit)
            End If

            s = s << 1
            If bit Then
                s += 1
            End If
        Next

        Return s
    End Function

    Public Sub ShiftEdc(bit As Boolean)
        If bit Then
            m_edc = m_edc Xor 1 << 15
        Else
            m_edc = m_edc Xor 0 << 15
        End If

        Dim control = m_edc >> 15

        m_edc = m_edc Xor control << 4
        m_edc = m_edc Xor control << 11

        m_edc = m_edc << 1

        If control = 1 Then
            m_edc = m_edc Or 1
        Else
            m_edc = m_edc And &HFFFE
        End If
    End Sub

    Public Sub ResetEdc()
        m_edc = &HFFFF
    End Sub

#End Region

End Class
