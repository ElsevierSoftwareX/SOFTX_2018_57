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
Imports EwEPlugin
Imports EwEUtils.Database
Imports EwEUtils.Core

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.1.1.002:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Fixed MSY group and fleet year constraints.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_01_01_002
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Enumrated type containing the world's 3-letter ISO currency codes.
    ''' </summary>
    ''' <remarks>
    ''' Last time we'll ever need this. From now on, ISO currency codes are
    ''' obtained from the .NET framework.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Private Enum eUnitMonetaryType As Integer
        ''' <summary>Currency type is not set.</summary>
        NotSet = 0
        ''' <summary>UAE Dirham</summary>
        AED
        ''' <summary>Afghanistan Afghani</summary>
        AFN
        ''' <summary>Albanian Lek</summary>
        ALL
        ''' <summary>Armenian Dram</summary>
        AMD
        ''' <summary>Antillean Guilder</summary>
        ANG ' 	
        ''' <summary>Angolan New Kwanza</summary>
        AOR ' 	
        ''' <summary>Argentinian Peso</summary>
        ARS ' 	
        ''' <summary>Australian Dollar</summary>
        AUD ' 	
        ''' <summary>Aruban Florin</summary>
        AWG ' 	
        ''' <summary>Azerbaijan Manat</summary>
        AZM ' 	
        ''' <summary>Bosnian Konvertibilna Marka</summary>
        BAM ' 	
        ''' <summary>Barbadian Dollar</summary>
        BBD ' 	
        ''' <summary>Bangladesh Taka</summary>
        BDT ' 	
        ''' <summary>Bulgarian New Lev</summary>
        BGN ' 	
        ''' <summary>Bahraini Dinar</summary>
        BHD ' 	
        ''' <summary>Burundian Franc</summary>
        BIF ' 	
        ''' <summary>Bermudan Dollar</summary>
        BMD ' 	
        ''' <summary>Brunei Dollar</summary>
        BND ' 	
        ''' <summary>Bolivian Boliviano</summary>
        BOB ' 	
        ''' <summary>Brazilian Real</summary>
        BRL ' 	
        ''' <summary>Bahamas Dollar</summary>
        BSD ' 	
        ''' <summary>Bhutan Ngultrum</summary>
        BTN ' 	
        ''' <summary>Botswana Pula</summary>
        BWP ' 	
        ''' <summary>Belarussian Ruble</summary>
        BYB ' 	
        ''' <summary>Belizean Dollar</summary>
        BZD ' 	
        ''' <summary>Canadian Dollar</summary>
        CAD ' 	
        ''' <summary>Congolese Franc</summary>
        CDF ' 	
        ''' <summary>Swiss Franc</summary>
        CHF ' 	
        ''' <summary>Chilean Peso</summary>
        CLP ' 	
        ''' <summary>Chinese Yuan Renminbi</summary>
        CNY ' 	
        ''' <summary>Colombian Peso</summary>
        COP ' 	
        ''' <summary>Costa Rican Colon</summary>
        CRC ' 	
        ''' <summary>Cuban Peso</summary>
        CUP ' 	
        ''' <summary>Cape Verdean Escudo</summary>
        CVE ' 	
        ''' <summary>Czech Koruna</summary>
        CZK ' 	
        ''' <summary>Djiboutian Franc</summary>
        DJF ' 	
        ''' <summary>Danish Krone</summary>
        DKK ' 	
        ''' <summary>Dominican Republic Peso</summary>
        DOP ' 	
        ''' <summary>Algerian Dinar</summary>
        DZD ' 	
        ''' <summary>Ecuador Sucre</summary>
        ECS ' 	
        ''' <summary>Estonian Kroon</summary>
        EEK ' 	
        ''' <summary>Eqyptian Pound</summary>
        EGP ' 	
        ''' <summary>Ethiopian Birr</summary>
        ETB ' 	
        ''' <summary>Euro</summary>
        EUR ' 	
        ''' <summary>Fijian Dollar</summary>
        FJD ' 	
        ''' <summary>Falkland Islands Pound</summary>
        FKP ' 	
        ''' <summary>French Franc</summary>
        FRF ' 	
        ''' <summary>UK Pound Sterling</summary>
        GBP ' 	
        ''' <summary>Georgian Lari</summary>
        GEL ' 	
        ''' <summary>Ghana Cedi</summary>
        GHC ' 	
        ''' <summary>Gibraltarian Pound</summary>
        GIP ' 	
        ''' <summary>Gambian Dalasi</summary>
        GMD ' 	
        ''' <summary>Guinean Franc</summary>
        GNF ' 	
        ''' <summary>Guatemalan Quetzal</summary>
        GTQ ' 	
        ''' <summary>Guyanese Dollar</summary>
        GYD ' 	
        ''' <summary>Hong Kong Dollar</summary>
        HKD ' 	
        ''' <summary>Honduran Lempira</summary>
        HNL ' 	
        ''' <summary>Croatian Kuna</summary>
        HRK ' 	
        ''' <summary>Haitian Gourde</summary>
        HTG ' 	
        ''' <summary>Hungarian Forint</summary>
        HUF ' 	
        ''' <summary>Indonesian Rupiah</summary>
        IDR ' 	
        ''' <summary>Israeli New Sheqel</summary>
        ILS ' 	
        ''' <summary>Indian Rupee</summary>
        INR ' 	
        ''' <summary>Iraqi Dinar</summary>
        IQD ' 	
        ''' <summary>Iranian Rial</summary>
        IRR ' 	
        ''' <summary>Iceland Krona</summary>
        ISK ' 	
        ''' <summary>Jamaican Dollar</summary>
        JMD ' 	
        ''' <summary>Jordanian Dinar</summary>
        JOD ' 	
        ''' <summary>Japanese Yen</summary>
        JPY ' 	
        ''' <summary>Kenyan Shilling</summary>
        KES ' 	
        ''' <summary>Kyrgyzstan Som</summary>
        KGS ' 	
        ''' <summary>Cambodian Riel</summary>
        KHR ' 	
        ''' <summary>Comoran Franc</summary>
        KMF ' 	
        ''' <summary>Korean PR Won (N.Korea)</summary>
        KPW ' 	
        ''' <summary>Korean Republic Won (S.Korea)</summary>
        KRW ' 	
        ''' <summary>Kuwaiti Dinar</summary>
        KWD ' 	
        ''' <summary>Caymanian Dollar</summary>
        KYD ' 	
        ''' <summary>Kazakhstan Tenge</summary>
        KZT ' 	
        ''' <summary>Laos Kip</summary>
        LAK ' 	
        ''' <summary>Lebanese Pound</summary>
        LBP ' 	
        ''' <summary>Sri Lanka Rupee</summary>
        LKR ' 	
        ''' <summary>Liberian Dollar</summary>
        LRD ' 	
        ''' <summary>Lesothian Loti</summary>
        LSL ' 	
        ''' <summary>Lithuanian Litas</summary>
        LTL ' 	
        ''' <summary>Latvian Lat</summary>
        LVL ' 	
        ''' <summary>Libyan Dinar</summary>
        LYD ' 	
        ''' <summary>Moroccan Dirham</summary>
        MAD ' 	
        ''' <summary>Moldovan Leu</summary>
        MDL ' 	
        ''' <summary>Malagasy Franc</summary>
        MGF ' 	
        ''' <summary>Macedonian Denar</summary>
        MKD ' 	
        ''' <summary>Myanmar Kyat</summary>
        MMK ' 	
        ''' <summary>Mongolian Tugrik</summary>
        MNT ' 	
        ''' <summary>Macau Pataca</summary>
        MOP ' 	
        ''' <summary>Mauritanian Ougiya</summary>
        MRO ' 	
        ''' <summary>Mauritian Rupee</summary>
        MUR ' 	
        ''' <summary>Maldivian Rufiyaa</summary>
        MVR ' 	
        ''' <summary>Malawi Kwacha</summary>
        MWK ' 	
        ''' <summary>Mexican New Peso</summary>
        MXN ' 	
        ''' <summary>Malaysian Ringgit</summary>
        MYR ' 	
        ''' <summary>Mozambique Metical</summary>
        MZM ' 	
        ''' <summary>Namibia Dollar</summary>
        NAD ' 	
        ''' <summary>Nigerian Naira</summary>
        NGN ' 	
        ''' <summary>Nicaraguan Crdoba</summary>
        NIO ' 	
        ''' <summary>Norwegian Krone</summary>
        NOK ' 	
        ''' <summary>Nepalese Rupee</summary>
        NPR ' 	
        ''' <summary>New Zealand Dollar</summary>
        NZD ' 	
        ''' <summary>Omani Rial</summary>
        OMR ' 	
        ''' <summary>Panamanian Balboa</summary>
        PAB ' 	
        ''' <summary>Peruvian New Sol</summary>
        PEN ' 	
        ''' <summary>Papua New Guinea Kina</summary>
        PGK ' 	
        ''' <summary>Philippine Piso</summary>
        PHP ' 	
        ''' <summary>Pakistani Rupee</summary>
        PKR ' 	
        ''' <summary>Polish New Zloty</summary>
        PLN ' 	
        ''' <summary>Paraguayan Guaran</summary>
        PYG ' 	
        ''' <summary>Qatari Riyal</summary>
        QAR ' 	
        ''' <summary>Romanian Leu</summary>
        ROL ' 	
        ''' <summary>Serbian Dinar</summary>
        RSD ' 	
        ''' <summary>Russian Rouble</summary>
        RUB ' 	
        ''' <summary>Rwandan Franc</summary>
        RWF ' 	
        ''' <summary>Saudi Riyal</summary>
        SAR ' 	
        ''' <summary>Solomon Islands Dollar</summary>
        SBD ' 	
        ''' <summary>Seychelles Rupee</summary>
        SCR ' 	
        ''' <summary>Sudanese Dinar</summary>
        SDD ' 	
        ''' <summary>Swedish Krona</summary>
        SEK ' 	
        ''' <summary>Singaporean Dollar</summary>
        SGD ' 	
        ''' <summary>Saint Helena Pound</summary>
        SHP ' 	
        ''' <summary>Slovak Koruna</summary>
        SKK ' 	
        ''' <summary>Sierra Leone Leone</summary>
        SLL ' 	
        ''' <summary>Somali Shilling</summary>
        SOS ' 	
        ''' <summary>Surinamese Guilder</summary>
        SRG ' 	
        ''' <summary>Sáo Tome and Principe Dobra</summary>
        STD ' 	
        ''' <summary>Salvadoran Colon</summary>
        SVC ' 	
        ''' <summary>Syrian Pound</summary>
        SYP ' 	
        ''' <summary>Swaziland Lilangeni</summary>
        SZL ' 	
        ''' <summary>Thai Baht</summary>
        THB ' 	
        ''' <summary>Tajikistan Ruble</summary>
        TJR ' 	
        ''' <summary>Turkmenistan Manat</summary>
        TMM ' 	
        ''' <summary>Tunisian Dinar</summary>
        TND ' 	
        ''' <summary>Tonga Pa'anga</summary>
        TOP ' 	
        ''' <summary>New Turkish Lira</summary>
        [TRY] '
        ''' <summary>Trinidadian Dollar</summary>
        TTD ' 	
        ''' <summary>Taiwanese Yuan</summary>
        TWD ' 	
        ''' <summary>Tanzanian Shilling</summary>
        TZS ' 	
        ''' <summary>Ukraine Hryvnias</summary>
        UAH ' 	
        ''' <summary>Uganda New Shilling</summary>
        UGX ' 	
        ''' <summary>U.S. Dollar</summary>
        USD ' 	
        ''' <summary>Peso Uruguayo</summary>
        UYU ' 	
        ''' <summary>Uzbekistan Sum</summary>
        UZS ' 	
        ''' <summary>Venezuelan Bolvar</summary>
        VEB ' 	
        ''' <summary>Vietnamese New Dng</summary>
        VND ' 	
        ''' <summary>Vanuatu Vatu</summary>
        VUV ' 	
        ''' <summary>Western Samoan Tala</summary>
        WST ' 	
        ''' <summary>CFA Central Franc</summary>
        XAF ' 	
        ''' <summary>East Caribbean Dollar</summary>
        XCD ' 	
        ''' <summary>CFA West Franc</summary>
        XOF ' 	
        ''' <summary>French Polynesian CFA France (CFP)</summary>
        XPF ' 	
        ''' <summary>Yemen Rial</summary>
        YER ' 	
        ''' <summary>South African Rand</summary>
        ZAR ' 	
        ''' <summary>Zambian Kwacha</summary>
        ZMK ' 	
        ''' <summary>Zimbabwean Dollar</summary>
        ZWD ' 	

    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the update version number that will be entered in
    ''' the update log of the database. This version number is also used to check
    ''' whether an update should run.
    ''' </summary>
    ''' <remarks>
    ''' If <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> is provided, the
    ''' update is ran regardless of version number.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.101002!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the text that will be entered in the update log in
    ''' the database.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Monetary units obtained from .NET"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean

        Dim bSucces As Boolean = True
        Dim unitOld As eUnitMonetaryType = eUnitMonetaryType.NotSet

        Try
            unitOld = DirectCast(db.GetValue("SELECT UnitMonetary FROM EcopathModel"), eUnitMonetaryType)
        Catch ex As Exception
            ' Blarp
        End Try

        ' Provide default
        If unitOld = eUnitMonetaryType.NotSet Then
            unitOld = eUnitMonetaryType.EUR
        End If

        db.Execute("ALTER TABLE EcopathModel DROP COLUMN UnitMonetaryCustom")

        bSucces = db.Execute("ALTER TABLE EcopathModel DROP COLUMN UnitMonetary")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathModel ADD COLUMN UnitMonetary TEXT(10)")
        bSucces = bSucces And db.Execute("UPDATE EcopathModel SET UnitMonetary='" & unitOld.ToString & "'")
        Return bSucces

    End Function

End Class
