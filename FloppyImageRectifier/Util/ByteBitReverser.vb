Public Class ByteBitReverser

    Private Shared m_byteReverseByteMapping = New Dictionary(Of Byte, Byte)

    Shared Sub New()
        For b = 0 To 255
            Dim reverseByte = 0

            For j = 0 To 7
                If (b And 1 << j) > 0 Then
                    reverseByte += 1 << 7 - j
                End If
            Next
            m_byteReverseByteMapping(b) = reverseByte
        Next
    End Sub

    Public Shared Function ReverseBytes(data As List(Of Byte)) As List(Of Byte)
        Dim reversedBytes = New List(Of Byte)

        For Each b In data
            Dim reversedByte = m_byteReverseByteMapping(b)
            reversedBytes.Add(reversedByte)
        Next

        Return reversedBytes
    End Function
End Class
