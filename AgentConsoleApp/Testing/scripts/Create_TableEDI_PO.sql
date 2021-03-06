USE [Eordering]
GO
/****** Object:  Table [dbo].[EDI_POHEADER_LEVEL]    Script Date: 19/10/2563 13:56:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EDI_POHEADER_LEVEL](
	[FILE_CODE] [nvarchar](40) NOT NULL,
	[TOTAL_RECORDS] [smallint] NOT NULL,
	[PO_NUMBER] [nvarchar](40) NOT NULL,
	[PO_TYPE] [bit] NOT NULL,
	[CONTRACT_NUMBER] [nvarchar](40) NULL,
	[ORDERED_DATE] [datetime] NOT NULL,
	[DELIVERY_DATE] [datetime] NOT NULL,
	[HOSP_CODE] [nvarchar](40) NOT NULL,
	[HOSP_NAME] [nvarchar](80) NOT NULL,
	[BUYER_NAME] [nvarchar](100) NULL,
	[BUYER_DEPARTMENT] [nvarchar](100) NULL,
	[EMAIL] [nvarchar](40) NOT NULL,
	[SUPPLIER_CODE] [nvarchar](40) NOT NULL,
	[SHIP_TO_CODE] [nvarchar](40) NOT NULL,
	[BILL_TO_CODE] [nvarchar](40) NOT NULL,
	[Approval_Code] [nvarchar](20) NOT NULL,
	[Budget_Code] [nvarchar](20) NOT NULL,
	[CURRENCY_CODE] [nvarchar](20) NOT NULL,
	[PAYMENT_TERM] [nvarchar](80) NOT NULL,
	[DISCOUNT_PCT] [float] NOT NULL,
	[TOTAL_AMOUNT] [float] NOT NULL,
	[NOTE_TO_SUPPLIER] [nvarchar](500) NULL,
	[RESEND_FLAG] [nvarchar](40) NULL,
	[CREATION_DATE] [datetime] NOT NULL,
	[QUATATION_ID] [nvarchar](20) NULL,
	[CUSTOMER_ID] [nvarchar](20) NULL,
	[LAST_INTERFACED_DATE] [datetime] NOT NULL,
	[INTERFACE_ID] [nvarchar](20) NOT NULL,
	[File_Name] [nvarchar](100) NULL,
	[imp_Date] [datetime] NULL,
 CONSTRAINT [PK_EDI_POHEADER_LEVEL] PRIMARY KEY CLUSTERED 
(
	[PO_NUMBER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EDI_POLINE_LEVEL]    Script Date: 19/10/2563 13:56:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EDI_POLINE_LEVEL](
	[PO_NUMBER] [nvarchar](40) NOT NULL,
	[LINE_NUMBER] [smallint] NOT NULL,
	[HOSPITEM_CODE] [nvarchar](40) NOT NULL,
	[HOSPITEM_NAME] [nvarchar](max) NULL,
	[DISTITEM_CODE] [nvarchar](40) NOT NULL,
	[PACK_SIZE_DESC] [nvarchar](40) NULL,
	[ORDERED_QTY] [float] NOT NULL,
	[UOM] [nvarchar](20) NOT NULL,
	[PRICE_PER_UNIT] [float] NOT NULL,
	[LINE_AMOUNT] [float] NOT NULL,
	[DISCOUNT_LINE_ITEM] [float] NOT NULL,
	[URGENT_FLAG] [nvarchar](2) NOT NULL,
	[COMMENT] [nvarchar](255) NULL,
	[imp_Date] [datetime] NULL,
 CONSTRAINT [PK_EDI_POLINE_LEVEL] PRIMARY KEY CLUSTERED 
(
	[PO_NUMBER] ASC,
	[LINE_NUMBER] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[EDI_POHEADER_LEVEL] ADD  CONSTRAINT [DF_EDI_POHEADER LEVEL_imp_Date]  DEFAULT (getdate()) FOR [imp_Date]
GO
ALTER TABLE [dbo].[EDI_POLINE_LEVEL] ADD  CONSTRAINT [DF_EDI_POLINE LEVEL_imp_Date]  DEFAULT (getdate()) FOR [imp_Date]
GO
