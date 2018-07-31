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
Imports EwECore.ValueWrapper
Imports EwEUtils.Core

#End Region ' Imports

''' <summary>
''' Class to encapsulate and expose ecopath model for a single model
''' </summary>
Public Class cEwEModel
    Inherits cCoreInputOutputBase

#Region " Constructor "

    Sub New(ByRef TheCore As cCore)
        MyBase.New(TheCore)

        Dim val As cValue
        Dim meta As cVariableMetaData
        Dim desc() As Char

        Try

            m_dataType = eDataTypes.EwEModel
            m_coreComponent = eCoreComponentType.EcoPath

            'default OK status used for setVariable
            'see comment setVariable(...)
            m_ValidationStatus = New cVariableStatus(Me, eStatusFlags.OK, "", eVarNameFlags.NotSet)

            ' Description - use private metadata to allow more than the standard 254 characters
            meta = New cVariableMetaData(60000)
            val = New cValue(New String(desc), eVarNameFlags.Description, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str, meta)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            val = New cValue(New String(desc), eVarNameFlags.Author, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Contact
            val = New cValue(New String(desc), eVarNameFlags.Contact, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Area
            val = New cValue(New Single, eVarNameFlags.Area, eStatusFlags.OK, eValueTypes.Sng)
            m_values.Add(val.varName, val)

            ' NumDigits
            val = New cValue(New Integer, eVarNameFlags.NumDigits, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' FirstYear
            val = New cValue(New Integer, eVarNameFlags.EcopathFirstYear, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' NumYears
            val = New cValue(New Integer, eVarNameFlags.EcopathNumYears, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' North
            val = New cValue(New Single, eVarNameFlags.North, eStatusFlags.OK, eValueTypes.Sng)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' South
            val = New cValue(New Single, eVarNameFlags.South, eStatusFlags.OK, eValueTypes.Sng)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' East
            val = New cValue(New Single, eVarNameFlags.East, eStatusFlags.OK, eValueTypes.Sng)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' West
            val = New cValue(New Single, eVarNameFlags.West, eStatusFlags.OK, eValueTypes.Sng)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' GroupDigits
            val = New cValue(New Boolean, eVarNameFlags.GroupDigits, eStatusFlags.OK, eValueTypes.Bool)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Time unit (enum)
            val = New cValue(New Integer, eVarNameFlags.UnitTime, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Time unit (text)
            val = New cValue(New String(desc), eVarNameFlags.UnitTimeCustomText, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Currency unit (enum)
            val = New cValue(New Integer, eVarNameFlags.UnitCurrency, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Currency unit (text)
            val = New cValue(New String(desc), eVarNameFlags.UnitCurrencyCustomText, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Monetary unit (enum)
            val = New cValue(New String(desc), eVarNameFlags.UnitMonetary, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Area unit (enum)
            val = New cValue(New Integer, eVarNameFlags.UnitArea, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Area unit (text)
            val = New cValue(New String(desc), eVarNameFlags.UnitAreaCustomText, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Map georeferencing unit (enum)
            val = New cValue(New Integer, eVarNameFlags.UnitMapRef, eStatusFlags.OK, eValueTypes.Int)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Country
            val = New cValue(New String(desc), eVarNameFlags.Country, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            val = New cValue(New String(desc), eVarNameFlags.EcosystemType, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Ecobase code
            val = New cValue(New String(desc), eVarNameFlags.CodeEcobase, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' DOI
            val = New cValue(New String(desc), eVarNameFlags.PublicationDOI, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            m_values.Add(val.varName, val)

            ' URI
            val = New cValue(New String(desc), eVarNameFlags.PublicationURI, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Reference
            val = New cValue(New String(desc), eVarNameFlags.PublicationReference, eStatusFlags.OK Or eStatusFlags.Null, eValueTypes.Str)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' Last saved julian date
            val = New cValue(New Double, eVarNameFlags.LastSaved, eStatusFlags.OK, eValueTypes.Sng)
            val.AffectsRunState = False
            m_values.Add(val.varName, val)

            ' IsEcopaceCoupled
            val = New cValue(New Boolean, eVarNameFlags.IsEcospaceModelCoupled, eStatusFlags.OK, eValueTypes.Bool)
            m_values.Add(val.varName, val)

            ' DiversityIndex (enum)
            val = New cValue(New Integer, eVarNameFlags.DiversityIndex, eStatusFlags.OK, eValueTypes.Int)
            m_values.Add(val.varName, val)

            'set status flags to their default values
            ResetStatusFlags()

        Catch ex As Exception
            Debug.Assert(False, "Error creating new cModel.")
            cLog.Write(ex, "cEwEModel.New()")
        End Try

    End Sub

#End Region ' Constructor

#Region " Variable via dot(.) operator "

    Public Property Description() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Description))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.Description, str)
        End Set
    End Property

    Public Property Author() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Author))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.Author, str)
        End Set
    End Property

    Public Property Contact() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Contact))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.Contact, str)
        End Set
    End Property

    Public Property Area() As Single
        Get
            Return CSng(GetVariable(eVarNameFlags.Area))
        End Get

        Set(ByVal sArea As Single)
            SetVariable(eVarNameFlags.Area, sArea)
        End Set
    End Property

    Public Property NumDigits() As Integer
        Get
            Return CInt(GetVariable(eVarNameFlags.NumDigits))
        End Get

        Set(ByVal iNumDigits As Integer)
            SetVariable(eVarNameFlags.NumDigits, iNumDigits)
        End Set
    End Property

    Public Property GroupDigits() As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.GroupDigits))
        End Get

        Set(ByVal bGroupDigits As Boolean)
            SetVariable(eVarNameFlags.GroupDigits, bGroupDigits)
        End Set
    End Property

    Public Property DiversityIndexType() As eDiversityIndexType
        Get
            Return DirectCast(GetVariable(eVarNameFlags.DiversityIndex), eDiversityIndexType)
        End Get

        Set(ByVal i As eDiversityIndexType)
            SetVariable(eVarNameFlags.DiversityIndex, CInt(i))
        End Set
    End Property

    Public Property UnitTime() As eUnitTimeType
        Get
            Return DirectCast(GetVariable(eVarNameFlags.UnitTime), eUnitTimeType)
        End Get

        Set(ByVal i As eUnitTimeType)
            SetVariable(eVarNameFlags.UnitTime, CInt(i))
        End Set
    End Property

    Public Property UnitTimeCustomText() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.UnitTimeCustomText))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.UnitTimeCustomText, str)
        End Set
    End Property

    Public Property UnitCurrency() As eUnitCurrencyType
        Get
            Return DirectCast(GetVariable(eVarNameFlags.UnitCurrency), eUnitCurrencyType)
        End Get

        Set(ByVal i As eUnitCurrencyType)
            SetVariable(eVarNameFlags.UnitCurrency, CInt(i))
        End Set
    End Property

    Public Property UnitCurrencyCustomText() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.UnitCurrencyCustomText))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.UnitCurrencyCustomText, str)
        End Set
    End Property

    Public Property UnitMonetary() As String
        Get
            Return DirectCast(GetVariable(eVarNameFlags.UnitMonetary), String)
        End Get

        Set(ByVal strUnit As String)
            SetVariable(eVarNameFlags.UnitMonetary, strUnit)
        End Set
    End Property

    Public Property UnitArea() As eUnitAreaType
        Get
            Return DirectCast(GetVariable(eVarNameFlags.UnitArea), eUnitAreaType)
        End Get

        Set(ByVal i As eUnitAreaType)
            SetVariable(eVarNameFlags.UnitArea, CInt(i))
        End Set
    End Property

    Public Property UnitAreaCustomText() As String
        Get
            Return CStr(GetVariable(eVarNameFlags.UnitAreaCustomText))
        End Get

        Set(ByVal str As String)
            SetVariable(eVarNameFlags.UnitAreaCustomText, str)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the first year that a model represents.
    ''' </summary>
    Public Property FirstYear() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.EcopathFirstYear))
        End Get

        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.EcopathFirstYear, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the number of years that a model represents.
    ''' </summary>
    Public Property NumYears() As Integer
        Get
            Return CInt(Me.GetVariable(eVarNameFlags.EcopathNumYears))
        End Get

        Set(ByVal value As Integer)
            Me.SetVariable(eVarNameFlags.EcopathNumYears, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the southern extent of the model bounding box in decimal degrees.
    ''' </summary>
    Public Property South() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.South))
        End Get

        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.South, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the northern extent of the model bounding box in decimal degrees.
    ''' </summary>
    Public Property North() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.North))
        End Get

        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.North, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the western extent of the model bounding box in decimal degrees.
    ''' </summary>
    Public Property West() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.West))
        End Get

        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.West, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the eastern extent of the model bounding box in decimal degrees.
    ''' </summary>
    Public Property East() As Single
        Get
            Return CSng(Me.GetVariable(eVarNameFlags.East))
        End Get

        Set(ByVal value As Single)
            Me.SetVariable(eVarNameFlags.East, value)
        End Set
    End Property

    ''' <summary>
    ''' Get/set the top left extent of the model bounding box in decimal degrees.
    ''' </summary>
    Public Property PosTopLeft As Drawing.PointF
        Get
            Return New Drawing.PointF(Me.West, Me.North)
        End Get
        Set(value As Drawing.PointF)
            Me.West = value.X
            Me.North = value.Y
        End Set
    End Property

    ''' <summary>
    ''' Get/set the bottom right extent of the model bounding box in decimal degrees.
    ''' </summary>
    Public Property PosBottomRight As Drawing.PointF
        Get
            Return New Drawing.PointF(Me.East, Me.South)
        End Get
        Set(value As Drawing.PointF)
            Me.East = value.X
            Me.South = value.Y
        End Set
    End Property

    ''' <summary>
    ''' Get/set the Julian date the model was last saved.
    ''' </summary>
    Public Property LastSaved() As Double
        Get
            Return CDbl(GetVariable(eVarNameFlags.LastSaved))
        End Get

        Set(ByVal value As Double)
            SetVariable(eVarNameFlags.LastSaved, value)
        End Set
    End Property

    Public Property IsEcoSpaceModelCoupled() As Boolean
        Get
            Return CBool(GetVariable(eVarNameFlags.IsEcospaceModelCoupled))
        End Get

        Set(ByVal value As Boolean)
            SetVariable(eVarNameFlags.IsEcospaceModelCoupled, value)
        End Set
    End Property

    Public Property Country As String
        Get
            Return CStr(GetVariable(eVarNameFlags.Country))
        End Get

        Set(ByVal value As String)
            SetVariable(eVarNameFlags.Country, value)
        End Set
    End Property

    Public Property EcosystemType As String
        Get
            Return CStr(GetVariable(eVarNameFlags.EcosystemType))
        End Get

        Set(ByVal value As String)
            SetVariable(eVarNameFlags.EcosystemType, value)
        End Set
    End Property

    Public Property EcobaseCode As String
        Get
            Return CStr(GetVariable(eVarNameFlags.CodeEcobase))
        End Get

        Set(ByVal value As String)
            SetVariable(eVarNameFlags.CodeEcobase, value)
        End Set
    End Property

    Public Property PublicationDOI As String
        Get
            Return CStr(GetVariable(eVarNameFlags.PublicationDOI))
        End Get

        Set(ByVal value As String)
            SetVariable(eVarNameFlags.PublicationDOI, value)
        End Set
    End Property

    Public Property PublicationURI As String
        Get
            Return CStr(GetVariable(eVarNameFlags.PublicationURI))
        End Get

        Set(ByVal value As String)
            SetVariable(eVarNameFlags.PublicationURI, value)
        End Set
    End Property

    Public Property PublicationReference As String
        Get
            Return CStr(GetVariable(eVarNameFlags.PublicationReference))
        End Get

        Set(ByVal value As String)
            SetVariable(eVarNameFlags.PublicationReference, value)
        End Set
    End Property

#End Region ' Variable via dot(.) operator

#Region " Status Flags via dot(.) operator"

    Public Property DescriptionStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.Description)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.Description, value)
        End Set

    End Property

    Public Property NumDigitsStatus() As eStatusFlags

        Get
            Return GetStatus(eVarNameFlags.NumDigits)
        End Get
        Set(ByVal value As eStatusFlags)
            SetStatus(eVarNameFlags.NumDigits, value)
        End Set

    End Property

    ' ToDo: all all other vars

#End Region ' Status Flags via dot(.) operator

End Class
