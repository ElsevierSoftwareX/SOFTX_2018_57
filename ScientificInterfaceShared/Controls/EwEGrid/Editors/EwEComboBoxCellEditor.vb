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

#Region " Imports "

Option Strict On
Imports EwEUtils.Utilities
Imports SourceGrid2.DataModels

#End Region ' Imports

Namespace Controls.EwEGrid

    <CLSCompliant(False)> _
    Public Class EwEComboBoxCellEditor
        Inherits EditorComboBox

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Create a combo box editor that shows a range of values obtained from 
        ''' a <see cref="ITypeFormatter"/>
        ''' </summary>
        ''' <param name="formatter">The <see cref="ITypeFormatter">type formatter</see> to link to.</param>
        ''' <param name="standardvalues">An optional (sub)set of values to present in the combo box.</param>
        ''' -------------------------------------------------------------------
        Public Sub New(ByVal formatter As ITypeFormatter, Optional ByVal standardvalues As ICollection = Nothing)

            MyBase.New(formatter.GetDescribedType)

            Dim mapping As New SourceLibrary.ComponentModel.Validator.ValueMapping()
            Dim lValues As New List(Of Object)
            Dim lRepresentations As New List(Of String)

            ' No standard values provided?
            If (standardvalues Is Nothing) Then
                ' #Yes: formatting an enum?
                If (formatter.GetDescribedType.IsEnum) Then
                    ' #Yes: auto-extract standard values
                    For Each key As Object In [Enum].GetValues(formatter.GetDescribedType)
                        lValues.Add(key)
                        lRepresentations.Add(formatter.GetDescriptor(key))
                    Next
                End If
            Else
                ' #No: add standard values
                For Each item As Object In standardvalues
                    lValues.Add(item)
                    lRepresentations.Add(formatter.GetDescriptor(item))
                Next
            End If

            Me.StandardValues = lValues
            Me.StandardValuesExclusive = True
            Me.AllowStringConversion = False
            Me.EditableMode = SourceGrid2.EditableMode.SingleClick Or SourceGrid2.EditableMode.Focus Or SourceGrid2.EditableMode.AnyKey

            mapping.ValueList = lValues
            mapping.DisplayStringList = lRepresentations
            mapping.BindValidator(Me)

        End Sub

        Protected Overrides Sub OnConvertingObjectToValue(ByVal e As SourceLibrary.ComponentModel.ConvertingObjectEventArgs)

            If (e.Value IsNot Nothing) Then
                If Not Me.ValueType.UnderlyingSystemType.IsAssignableFrom(e.Value.GetType) Then
                    Try
                        Dim iValue As Integer = 0
                        If TypeOf (e.Value) Is String Then
                            iValue = Integer.Parse(CStr(e.Value))
                        Else
                            iValue = CInt(e.Value)
                        End If
                        If Me.ValueType.IsEnum Then
                            If Not [Enum].IsDefined(Me.ValueType, iValue) Then
                                ' Clear!
                                e.Value = Me.StandardValueAtIndex(0)
                            Else
                                e.Value = [Enum].ToObject(Me.ValueType, iValue)
                            End If
                        Else
                            e.Value = iValue
                        End If
                    Catch ex As Exception

                    End Try
                End If
            End If

            MyBase.OnConvertingObjectToValue(e)
        End Sub

    End Class

End Namespace
