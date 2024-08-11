Public Class BitList

#Region "Fields"

    Private m_writeByte As Byte
    Private m_writeByteIndex As Integer
    Private m_writeByteBitIndex As Integer

    Private m_readByte As Byte
    Private m_readByteIndex As Integer
    Private m_readByteBitIndex As Integer

    Private m_savedReadByteIndex As Integer
    Private m_savedReadByteBitIndex As Integer

    Private m_totalBitCount As Long

#End Region

#Region "Constructors"

    Public Sub New()
        m_data = New List(Of Byte)()

        m_writeByte = 0
        m_writeByteIndex = 0
        m_writeByteBitIndex = 7
        m_totalBitCount = 0

        m_readByte = 0
        m_readByteIndex = 0
        m_readByteBitIndex = 7

        m_savedReadByteIndex = 0
        m_savedReadByteBitIndex = 7
    End Sub

    Public Sub New(dataBuffer As List(Of Byte), length As Long)
        Me.New()
        m_data = dataBuffer.ToList()

        m_totalBitCount = length
        m_writeByteIndex = length \ 8
        m_writeByteBitIndex = 7 - (length Mod 8)

        RefreshWriteByte()
    End Sub

#End Region

#Region "Properties"

    Private m_data As List(Of Byte)
    Public ReadOnly Property Data As List(Of Byte)
        Get
            Return m_data
        End Get
    End Property

    Public Property BitCount As Integer
        Get
            Return m_totalBitCount
        End Get
        Private Set
            m_totalBitCount = Value
        End Set
    End Property

    Public ReadOnly Property IsEof As Boolean
        Get
            Return ReadPosition >= m_totalBitCount
        End Get
    End Property

    Public Property ReadPosition As Long
        Get
            Return m_readByteIndex * 8 + (7 - m_readByteBitIndex)
        End Get
        Set(value As Long)
            m_readByteIndex = value \ 8
            m_readByteBitIndex = 7 - (value Mod 8)

            RefreshReadByte()
        End Set
    End Property

    Public ReadOnly Property WritePosition As Long
        Get
            Return m_writeByteIndex * 8 + (7 - m_writeByteBitIndex)
        End Get
    End Property

    Public ReadOnly Property SavedPosition As Long
        Get
            Return m_savedReadByteIndex * 8 + (7 - m_savedReadByteBitIndex)
        End Get
    End Property

#End Region

#Region "Public Methods"

    Public Sub AddBit(bit As Boolean)
        If m_writeByteBitIndex = 7 Then
            m_writeByte = 0
            Data.Add(m_writeByte)
        End If

        If bit Then
            m_writeByte += 1 << m_writeByteBitIndex
            Data(m_writeByteIndex) = m_writeByte
        End If

        IncrementWriteIndex()
    End Sub

    Public Sub AddBitToStart(value As Boolean)
        Dim msb As Boolean = value
        Dim lsb As Boolean
        For byteIndex = 0 To Data.Count - 1
            Dim b = Data(byteIndex)
            lsb = (b And 1) > 0

            b = b >> 1

            If msb Then
                b += &H80
            End If

            Data(byteIndex) = b

            msb = lsb
        Next

        If m_writeByteBitIndex = 7 Then
            AddBit(msb)
        Else
            IncrementWriteIndex()
            If m_writeByteBitIndex <> 7 Then
                m_writeByte = Data(m_writeByteIndex)
            End If
        End If
    End Sub

    Public Function RemoveBitFromStart() As Boolean
        If Data.Count = 0 Then
            Throw New InvalidOperationException("Unable to remove bit from empty BitList")
        End If

        Dim value = (Data(0) And &H80) > 0

        For byteIndex = 1 To Data.Count - 1
            Dim currentByte = Data(byteIndex)
            Dim previousByte = Data(byteIndex - 1)
            Dim msb = (currentByte And &H80) > 0

            previousByte = previousByte << 1

            If msb Then
                previousByte += 1
            End If

            Data(byteIndex - 1) = previousByte
        Next

        DecrementWriteIndex()
        If m_writeByteBitIndex = 7 Then
            Data.RemoveAt(Data.Count - 1)
            RefreshWriteByte()
        Else
            m_writeByte = Data(m_writeByteIndex)
            m_writeByte = m_writeByte << 1
            Data(m_writeByteIndex) = m_writeByte
        End If

        Return value
    End Function

    Public Function RemoveBitFromEnd() As Boolean
        If Data.Count = 0 Then
            Throw New InvalidOperationException("Unable to remove bit from empty bitlist")
        End If

        Dim value As Boolean
        If m_writeByteBitIndex = 7 Then
            m_writeByteIndex -= 1
            m_writeByte = Data(m_writeByteIndex)

            m_writeByteBitIndex = 0
            value = (m_writeByte And 1) > 0
            m_writeByte = m_writeByte And &HFE
            Data(m_writeByteIndex) = m_writeByte
        Else
            m_writeByteBitIndex += 1
            value = (m_writeByte And (1 << m_writeByteBitIndex)) > 0

            If m_writeByteBitIndex = 7 Then
                m_writeByte = 0
                Data.RemoveAt(m_writeByteIndex)
            Else
                m_writeByte = m_writeByte And (Not (1 << m_writeByteBitIndex))
                Data(m_writeByteIndex) = m_writeByte
            End If
        End If

        m_totalBitCount -= 1
        Return value
    End Function

    Public Sub AddBitList(bl As BitList)
        While Not bl.IsEof
            Dim value = bl.ReadBit()
            AddBit(value)
        End While
    End Sub

    Public Function ReadBit() As Boolean
        If IsEof Then
            Throw New InvalidOperationException("Reached end of stream")
        End If

        If m_readByteBitIndex = 7 Then
            m_readByte = Data(m_readByteIndex)
        End If

        Dim value = (m_readByte And (1 << m_readByteBitIndex)) > 0
        m_readByteBitIndex -= 1

        If m_readByteBitIndex < 0 Then
            m_readByteBitIndex = 7
            m_readByteIndex += 1
        End If

        Return value
    End Function

    Public Sub SavePosition()
        m_savedReadByteIndex = m_readByteIndex
        m_savedReadByteBitIndex = m_readByteBitIndex
    End Sub

    Public Sub RestorePosition()
        m_readByteIndex = m_savedReadByteIndex
        m_readByteBitIndex = m_savedReadByteBitIndex

        RefreshReadByte()
    End Sub

    Public Sub ResetRead()
        m_readByte = 0
        m_readByteIndex = 0
        m_readByteBitIndex = 7
    End Sub

    Public Function MatchPercentage(testData As BitList) As Double
        Dim matches = 0
        Dim totalTransitions = 0

        Me.ResetRead()
        testData.ResetRead()
        While Not Me.IsEof AndAlso Not testData.IsEof
            Dim meZeroCount = 0
            While Not Me.IsEof AndAlso Me.ReadBit() = False
                meZeroCount += 1
            End While

            Dim testZeroCount = 0
            While Not testData.IsEof AndAlso testData.ReadBit() = False
                testZeroCount += 1
            End While

            If meZeroCount = testZeroCount Then
                matches += 1
            End If

            totalTransitions += 1
        End While

        Return matches / totalTransitions
    End Function

    Public Function Find(minIndex As Long, maxIndex As Long, value As BitList, Optional debug As Boolean = False) As Long
        Dim index As Long = 0

        If debug Then
            Console.WriteLine($"Find value length {value.BitCount}")
        End If

        For index = minIndex To maxIndex
            value.ResetRead()
            ReadPosition = index
            Dim found = True
            Dim count = 0
            While Not value.IsEof
                If IsEof Then
                    found = False
                    Exit While
                End If

                Dim myValue = ReadBit()
                Dim checkValue = value.ReadBit()
                If myValue <> checkValue Then
                    found = False
                    If debug Then
                        Console.WriteLine($"Match not found for index {index} after {count} bits")
                    End If
                    Exit While
                End If
                count += 1
            End While

            If found Then
                If debug Then
                    Console.WriteLine($"Match found for index {index}")
                End If
                Return index
            End If
        Next

        Return -1
    End Function

    Public Function Slice(startIndex As Long, length As Long) As BitList
        Dim bl = New BitList()

        If length + startIndex > BitCount Then
            Throw New InvalidOperationException("Not enough bits in list for startIndex/Length combination")
        End If

        ReadPosition = startIndex

        For i = 0 To length - 1
            Dim value = ReadBit()
            bl.AddBit(value)
        Next

        Return bl
    End Function

    Public Sub ShiftBitsFromStartOf(bl As BitList, count As Long)
        For i = 0 To count - 1
            Dim bit = bl.RemoveBitFromStart()
            AddBit(bit)
        Next
    End Sub

    Public Sub ShiftBitsToStartOf(bl As BitList, count As Long)
        For i = 0 To count - 1
            Dim bit = RemoveBitFromEnd()
            bl.AddBitToStart(bit)
        Next
    End Sub

    Public Sub RotateRight(count As Long)
        For i = 0 To count - 1
            Dim bit = RemoveBitFromEnd()
            AddBitToStart(bit)
        Next
    End Sub

    Public Sub RotateLeft(count As Long)
        For i = 0 To count - 1
            Dim bit = RemoveBitFromStart()
            AddBit(bit)
        Next
    End Sub

#End Region

#Region "Private Methods"

    Private Sub IncrementWriteIndex()
        m_writeByteBitIndex -= 1
        If m_writeByteBitIndex < 0 Then
            m_writeByteBitIndex = 7
            m_writeByteIndex += 1
        End If
        m_totalBitCount += 1
    End Sub

    Private Sub DecrementWriteIndex()
        m_writeByteBitIndex += 1
        If m_writeByteBitIndex > 7 Then
            m_writeByteBitIndex = 0
            m_writeByteIndex -= 1
        End If
        m_totalBitCount -= 1
    End Sub

    Private Sub RefreshWriteByte()
        If m_writeByteBitIndex = 7 AndAlso m_writeByteIndex = 0 Then
            m_writeByte = 0
        ElseIf m_writeByteBitIndex = 7 AndAlso m_writeByteIndex > 0 Then
            m_writeByte = Data(m_writeByteIndex - 1)
        Else
            m_writeByte = Data(m_writeByteIndex)
        End If
    End Sub

    Private Sub RefreshReadByte()
        If m_readByteBitIndex = 7 AndAlso m_readByteIndex = 0 Then
            m_readByte = 0
        ElseIf m_readByteBitIndex = 7 AndAlso m_readByteIndex > 0 Then
            m_readByte = Data(m_readByteIndex - 1)
        Else
            m_readByte = Data(m_readByteIndex)
        End If
    End Sub

#End Region

End Class
