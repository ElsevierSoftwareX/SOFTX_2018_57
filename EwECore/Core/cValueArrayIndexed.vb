' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

Option Strict On
Imports EwEUtils.Core

Namespace ValueWrapper

    Public Class cValueArrayIndexed
        Inherits cValueArray

        Protected m_dataType As eDataTypes
        Protected m_iArrayIndex As Integer
        Shadows m_CounterDelegate As CoreIndexedCounterDelegate

        Public Property iSecondIndex As Integer


        ''' <summary>
        ''' Constructor with no validation object
        ''' </summary>
        ''' <param name="theValueType"></param>
        ''' <param name="VarName"></param>
        ''' <param name="Status"></param>
        ''' <param name="CounterType"></param>
        ''' <param name="CounterDelegate"></param>
        ''' <remarks></remarks>
        Sub New(ByVal theValueType As eValueTypes, ByVal VarName As eVarNameFlags, ByVal Status As eStatusFlags, ByVal CounterType As eCoreCounterTypes,
                ByRef CounterDelegate As CoreIndexedCounterDelegate, ByVal iArrayIndex As Integer, ByVal DataType As eDataTypes)
            MyBase.New(theValueType, VarName, Status, CounterType, Nothing)

            varType = theValueType
            m_varName = VarName
            m_dataType = DataType
            m_iArrayIndex = iArrayIndex

            m_CounterDelegate = CounterDelegate
            m_Countertype = CounterType

            If SetSize() Then 'this will redim the arrays and set m_nObjects
                For i As Integer = 0 To m_nObjects
                    m_statusarray(i) = Status
                Next
                'Else
                '    Debug.Assert(False, "Something is wrong in " & Me.ToString & ".New()")
            End If

        End Sub


        ''' <summary>
        ''' Set the size of the array to the value in the cores data counter i.e. nGroups
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>This will only dimension the array data if the core counter is of a different size then the existing data.
        '''  Once the data has been resized it will need to be repopulated.</remarks>
        Public Overrides Function SetSize() As Boolean

            If m_CounterDelegate IsNot Nothing Then

                Dim newsize As Integer = m_CounterDelegate(m_Countertype, m_iArrayIndex)

                'only resize the data if it is different
                If newsize <> m_nObjects Then
                    m_nObjects = newsize
                    Select Case Me.varType
                        Case eValueTypes.BoolArray
                            Dim s(m_nObjects) As Boolean
                            m_values = s
                        Case eValueTypes.IntArray
                            Dim s(m_nObjects) As Integer
                            m_values = s
                        Case eValueTypes.SingleArray
                            Dim s(m_nObjects) As Single
                            m_values = s
                    End Select

                    ReDim m_statusarray(m_nObjects)

                End If

                Return True

            Else
                'System.Console.WriteLine(Me.ToString & ".setSize() not implemented.")
                'When a cValueArrayIndexed object in constructed it will call the base class constructor will a null m_CounterDelegate
                'which in turn calls this method before cValueArrayIndexed has had a chance to set m_CounterDelegate
                Return False
            End If

        End Function


        Public Overrides Sub Dispose()
            MyBase.Dispose()

            Me.m_CounterDelegate = Nothing

        End Sub

    End Class



End Namespace
