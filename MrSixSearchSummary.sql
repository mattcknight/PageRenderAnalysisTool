USE [Test]
GO

/****** Object:  Table [dbo].[MrSixSearchSummary]    Script Date: 07/30/2012 15:20:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[MrSixSearchSummary](
	[LogDay] [date] NOT NULL,
	[SearchName] [varchar](50) NOT NULL,
	[Total] [int] NOT NULL,
	[TotalLoggedIn] [int] NOT NULL,
	[TotalNonLoggedIn] [int] NOT NULL,
	[TotalZero] [int] NOT NULL,
	[TotalZeroLoggedIn] [int] NOT NULL,
	[TotalZeroNonLoggedIn] [int] NOT NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


