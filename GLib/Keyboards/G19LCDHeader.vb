﻿Module G19LCDHeader
    Friend ReadOnly G19LCDDataHeader As Byte() = {&H10, &HF, &H0, &H58, &H2, &H0, &H1, &H0, &H0, &H0, &H0, &H3F, &H1, &HEF, &H0, &HF, &H10, &H11, &H12, &H13, &H14, &H15, &H16, &H17, &H18, &H19, &H1A, &H1B, &H1C, &H1D, &H1E, &H1F, &H20, &H21, &H22, &H23, &H24, &H25, &H26, &H27, &H28, &H29, &H2A, &H2B, &H2C, &H2D, &H2E, &H2F, &H30, &H31, &H32, &H33, &H34, &H35, &H36, &H37, &H38, &H39, &H3A, &H3B, &H3C, &H3D, &H3E, &H3F, &H40, &H41, &H42, &H43, &H44, &H45, &H46, &H47, &H48, &H49, &H4A, &H4B, &H4C, &H4D, &H4E, &H4F, &H50, &H51, &H52, &H53, &H54, &H55, &H56, &H57, &H58, &H59, &H5A, &H5B, &H5C, &H5D, &H5E, &H5F, &H60, &H61, &H62, &H63, &H64, &H65, &H66, &H67, &H68, &H69, &H6A, &H6B, &H6C, &H6D, &H6E, &H6F, &H70, &H71, &H72, &H73, &H74, &H75, &H76, &H77, &H78, &H79, &H7A, &H7B, &H7C, &H7D, &H7E, &H7F, &H80, &H81, &H82, &H83, &H84, &H85, &H86, &H87, &H88, &H89, &H8A, &H8B, &H8C, &H8D, &H8E, &H8F, &H90, &H91, &H92, &H93, &H94, &H95, &H96, &H97, &H98, &H99, &H9A, &H9B, &H9C, &H9D, &H9E, &H9F, &HA0, &HA1, &HA2, &HA3, &HA4, &HA5, &HA6, &HA7, &HA8, &HA9, &HAA, &HAB, &HAC, &HAD, &HAE, &HAF, &HB0, &HB1, &HB2, &HB3, &HB4, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HBB, &HBC, &HBD, &HBE, &HBF, &HC0, &HC1, &HC2, &HC3, &HC4, &HC5, &HC6, &HC7, &HC8, &HC9, &HCA, &HCB, &HCC, &HCD, &HCE, &HCF, &HD0, &HD1, &HD2, &HD3, &HD4, &HD5, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, &HDC, &HDD, &HDE, &HDF, &HE0, &HE1, &HE2, &HE3, &HE4, &HE5, &HE6, &HE7, &HE8, &HE9, &HEA, &HEB, &HEC, &HED, &HEE, &HEF, &HF0, &HF1, &HF2, &HF3, &HF4, &HF5, &HF6, &HF7, &HF8, &HF9, &HFA, &HFB, &HFC, &HFD, &HFE, &HFF, &H0, &H1, &H2, &H3, &H4, &H5, &H6, &H7, &H8, &H9, &HA, &HB, &HC, &HD, &HE, &HF, &H10, &H11, &H12, &H13, &H14, &H15, &H16, &H17, &H18, &H19, &H1A, &H1B, &H1C, &H1D, &H1E, &H1F, &H20, &H21, &H22, &H23, &H24, &H25, &H26, &H27, &H28, &H29, &H2A, &H2B, &H2C, &H2D, &H2E, &H2F, &H30, &H31, &H32, &H33, &H34, &H35, &H36, &H37, &H38, &H39, &H3A, &H3B, &H3C, &H3D, &H3E, &H3F, &H40, &H41, &H42, &H43, &H44, &H45, &H46, &H47, &H48, &H49, &H4A, &H4B, &H4C, &H4D, &H4E, &H4F, &H50, &H51, &H52, &H53, &H54, &H55, &H56, &H57, &H58, &H59, &H5A, &H5B, &H5C, &H5D, &H5E, &H5F, &H60, &H61, &H62, &H63, &H64, &H65, &H66, &H67, &H68, &H69, &H6A, &H6B, &H6C, &H6D, &H6E, &H6F, &H70, &H71, &H72, &H73, &H74, &H75, &H76, &H77, &H78, &H79, &H7A, &H7B, &H7C, &H7D, &H7E, &H7F, &H80, &H81, &H82, &H83, &H84, &H85, &H86, &H87, &H88, &H89, &H8A, &H8B, &H8C, &H8D, &H8E, &H8F, &H90, &H91, &H92, &H93, &H94, &H95, &H96, &H97, &H98, &H99, &H9A, &H9B, &H9C, &H9D, &H9E, &H9F, &HA0, &HA1, &HA2, &HA3, &HA4, &HA5, &HA6, &HA7, &HA8, &HA9, &HAA, &HAB, &HAC, &HAD, &HAE, &HAF, &HB0, &HB1, &HB2, &HB3, &HB4, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HBB, &HBC, &HBD, &HBE, &HBF, &HC0, &HC1, &HC2, &HC3, &HC4, &HC5, &HC6, &HC7, &HC8, &HC9, &HCA, &HCB, &HCC, &HCD, &HCE, &HCF, &HD0, &HD1, &HD2, &HD3, &HD4, &HD5, &HD6, &HD7, &HD8, &HD9, &HDA, &HDB, &HDC, &HDD, &HDE, &HDF, &HE0, &HE1, &HE2, &HE3, &HE4, &HE5, &HE6, &HE7, &HE8, &HE9, &HEA, &HEB, &HEC, &HED, &HEE, &HEF, &HF0, &HF1, &HF2, &HF3, &HF4, &HF5, &HF6, &HF7, &HF8, &HF9, &HFA, &HFB, &HFC, &HFD, &HFE, &HFF}
End Module
