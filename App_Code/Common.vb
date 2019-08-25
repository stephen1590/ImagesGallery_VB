Imports System.Data
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic

Public Class Common

    Shared Function RunQuery(args As String, paramArrayList As ArrayList) As ArrayList
        Dim pokemonArray As New ArrayList
        Try
            Dim sqlConnection As New SqlConnection("Data Source=localhost;Initial Catalog=Pokemon;Integrated Security=SSPI;Trusted_Connection=yes;")
            Dim cmd As New SqlCommand
            Dim reader As SqlDataReader

            cmd.CommandText = args
            cmd.CommandType = CommandType.Text
            cmd.Connection = sqlConnection
            sqlConnection.Open()

            If paramArrayList IsNot Nothing Then
                For Each param In paramArrayList
                    cmd.Parameters.Add(param)
                Next
                cmd.Prepare()
            End If

            reader = cmd.ExecuteReader()

            While (reader.Read())
                pokemonArray.Add(ContentsGetWithHashTable(CType(reader, IDataRecord)))
            End While

            reader.Close()
            sqlConnection.Close()

        Catch Ex As Exception
            Dim temp As New ArrayList
            temp.Add("Bad Query " + Ex.ToString)
            pokemonArray.Add(temp)
            Return pokemonArray
        End Try

        Return pokemonArray
    End Function

    Shared Function ContentsGet(ByVal record As IDataRecord) As ArrayList
        Dim retVal As New ArrayList
        Dim count As Int16 = 0
        While count < record.FieldCount()
            retVal.Add(record(count).ToString())
            count += 1
        End While
        Return retVal
    End Function

    Shared Function ContentsGetWithHashTable(ByVal record As IDataRecord) As Hashtable
        Dim retVal As New Hashtable
        Dim count As Int16 = 0
        While count < record.FieldCount()
            retVal(record.GetName(count)) = (record(count).ToString())
            count += 1
        End While
        Return retVal
    End Function

    Shared Function CreateMD5StringFromFile(ByVal Filename As String) As String

        Dim MD5 = System.Security.Cryptography.MD5.Create
        Dim Hash As Byte()
        Dim sb As New System.Text.StringBuilder

        Using st As New IO.FileStream(Filename, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            Hash = MD5.ComputeHash(st)
        End Using

        For Each b In Hash
            sb.Append(b.ToString("X2"))
        Next

        Return sb.ToString
    End Function

End Class
