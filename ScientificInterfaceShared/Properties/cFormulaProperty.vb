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

Imports System.Globalization
Imports EwEUtils.Core
Imports ScientificInterfaceShared.Style

#End Region ' Imports

Namespace Properties

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Implements a property that is capable of calculating simple formulas.
    ''' </summary>
    ''' <remarks>
    ''' <para>The fomulas in cFormulaProperty accept <see cref="cProperty">Properties</see>
    ''' as operands, making this contracption extremely powerful for performing spreadsheet-
    ''' like calculations. Whenever an operand in the formula changes, this property will 
    ''' automatically recalculate the formula result. The formula result is available 
    ''' through <see cref="cProperty.GetValue">cProperty.GetValue()</see>. Additionally, 
    ''' formula updates will be broadcasted through the cProperty 
    ''' <see cref="cProperty.PropertyChanged">change event</see>.</para>
    ''' </remarks>
    ''' <example>
    ''' <para>To calculate 1 / (prop1 + prop2):</para>
    ''' <code>
    ''' Dim opAdd As New cBinaryOperation(eOperatorType.Add, prop1, prop2)
    ''' Dim opDiv As New cBinaryOperation(eOperatorType.Divide, 1, opAdd)
    ''' Dim propF As New cFormulaProperty(opDiv)
    ''' </code>
    ''' <para>To calculate Sqrt((propB^2) - (4*propA*propC)) / (2*propA):</para>
    ''' <code>
    ''' Dim opB2 As New cBinaryOperation(eOperatorType.Pow, propB, 2)
    ''' Dim opAC As New cBinaryOperation(eOperatorType.Multiply, propA, propC)
    ''' Dim op4AC As New cBinaryOperation(eOperatorType.Multiply, 4, opAC)
    ''' Dim opB2_4AC As New cBinaryOperation(eOperatorType.Substract, opB2, op4AC)
    ''' Dim op2A As New cBinaryOperation(eOperatorType.Multiply, 2, propA)
    ''' Dim formula As new cBinaryOperation(eOperatorType.Divide, opB2_4AC, op2A)
    ''' Dim propF as new cFormulaProperty(formula)
    ''' </code>
    ''' </example>
    ''' -----------------------------------------------------------------------
    Public Class cFormulaProperty
        : Inherits cSingleProperty

        ''' <summary>The formula.</summary>
        Private m_formula As cExpression = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Constructor, initializes a new cFormulaProperty instance.
        ''' </summary>
        ''' <param name="formula">The formula that will feed the value and
        ''' status of this <see cref="cProperty">Property</see>.</param>
        ''' -------------------------------------------------------------------
        Friend Sub New(ByVal formula As cExpression)
            MyBase.New()

            ' Sanity check
            Debug.Assert(formula IsNot Nothing, "Need valid formula")
            ' Store formula
            Me.m_formula = formula
            ' Listen to formula changes
            AddHandler m_formula.OnValueChanged, AddressOf OnFormulaChanged
            ' Initialize value
            Me.Calculate()

        End Sub

        Protected Friend Overrides Sub Dispose(ByVal bDisposing As Boolean)
            RemoveHandler m_formula.OnValueChanged, AddressOf OnFormulaChanged
            Me.m_formula.Dispose()
            Me.m_formula = Nothing
            MyBase.Dispose(bDisposing)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Attempt to get a valid <see cref="cExpression">cExpression</see> 
        ''' for a provided operand.
        ''' </summary>
        ''' <param name="operand">The operand to analyze.</param>
        ''' <returns>A valid <see cref="cExpression">cExpression</see>, or 
        ''' Nothing if the conversion could not be made.</returns>
        ''' <remarks>Accepted operand types are numerical values, 
        ''' <see cref="cSingleProperty">cSingleProperty</see> instances,
        ''' or <see cref="cExpression">cExpression-derived</see> objects.</remarks>
        ''' -------------------------------------------------------------------
        Public Shared Function GetExpression(ByVal operand As Object) As cExpression

            Dim s As Single = 0.0

            Debug.Assert(operand IsNot Nothing, "Operand is NULL, cannot proceed")

            ' Is operand already an Expression?
            If TypeOf (operand) Is cExpression Then
                ' #Yes: return type-casted operand
                Return DirectCast(operand, cExpression)
            End If

            ' Is operand a SingleProperty?
            If (TypeOf (operand) Is cSingleProperty) Then
                ' #Yes: wrap operand in a cPropertyOperand
                Return New cPropertyOperand(DirectCast(operand, cSingleProperty))
            End If

            ' Is operand a BooleanProperty?
            If (TypeOf (operand) Is cBooleanProperty) Then
                ' #Yes: wrap operand in a cPropertyOperand
                Return New cPropertyOperand(DirectCast(operand, cBooleanProperty))
            End If

            ' Is operand a boolean?
            If (TypeOf (operand) Is Boolean) Then
                Return New cStaticOperand(CSng(operand))
            End If

            ' Is operand convertable into a Single value?
            If Single.TryParse(operand.ToString(), Globalization.NumberStyles.Any, NumberFormatInfo.InvariantInfo, s) Then
                ' #Yes: wrap operand in a cStaticOperand
                Return New cStaticOperand(s)
            End If

            ' Unable to convert or wrap operand into an Expression: we're out of options
            Debug.Assert(False, String.Format("Unable to wrap or convert operand {0} of type {1}", operand.ToString(), operand.GetType()))

            ' Return failure
            Return Nothing

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Calculates the formula result.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub Calculate()

            Dim styleSum As cStyleGuide.eStyleFlags = (cStyleGuide.eStyleFlags.Sum Or cStyleGuide.eStyleFlags.NotEditable)
            Dim cf As cProperty.eChangeFlags = 0
            Dim sValue As Single = 0.0
            Dim style As cStyleGuide.eStyleFlags = cStyleGuide.eStyleFlags.OK

            Try
                ' Try to calculate formula outcome
                sValue = Me.m_formula.GetValue()
                ' Try to calculate formula style
                styleSum = (styleSum Or Me.m_formula.GetStyle())

            Catch ex As Exception
                ' Woops, something went wrong. For now, do not try to discover the error, just flag
                ' the Property value as erroneous
                styleSum = styleSum Or cStyleGuide.eStyleFlags.ErrorEncountered
                ' Reset the value
                sValue = 0.0
            End Try

            ' Update style without notifying anyone
            ' - Some core states are suppressed, such as NULL, Remarks, and ValueComputed
            If (Me.SetStyle(styleSum And Not (cStyleGuide.eStyleFlags.Remarks Or cStyleGuide.eStyleFlags.Null Or cStyleGuide.eStyleFlags.ValueComputed), TriState.False)) Then
                cf = cf Or eChangeFlags.CoreStatus
            End If

            ' Update value without notifying anyone
            If (Me.SetValue(sValue, TriState.False)) Then
                cf = cf Or eChangeFlags.Value
            End If

            ' Anything changed?
            If (cf = 0) Then
                ' #No: done
                Return
            End If

            ' Fire change notification
            Me.FireChangeNotification(cf)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Event handler, responds to operand value change events by recalculating
        ''' the formula result.
        ''' </summary>
        ''' -------------------------------------------------------------------
        Private Sub OnFormulaChanged(ByVal formula As cExpression)
            Me.Calculate()
        End Sub

    End Class

End Namespace
