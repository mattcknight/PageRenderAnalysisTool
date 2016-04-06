USE [Test]
GO

/****** Object:  Table [dbo].[PageThresholdCallStack]    Script Date: 07/26/2012 12:42:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PageThresholdCallStack](
	[CallStackId] [int] NOT NULL,
	[CallStack] [varchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CallStackId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

USE [Test]
GO

/****** Object:  Table [dbo].[PageThresholdMessageSource]    Script Date: 07/26/2012 12:43:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PageThresholdMessageSource](
	[MessageSourceId] [int] NOT NULL,
	[MessageSource] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MessageSourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


USE [Test]
GO

/****** Object:  Table [dbo].[PageThresholdHeaderTest]    Script Date: 07/26/2012 12:43:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PageThresholdHeaderTest](
	[SeqNum] [int] NOT NULL,
	[LogEntryId] [int] NOT NULL,
	[LogEntryDateTime] [datetime] NOT NULL,
	[WebServer] [varchar](128) NOT NULL,
	[PageCode] [tinyint] NOT NULL,
	[ExecutionTime] [bigint] NOT NULL,
	[DbTime] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[HasLoginAuth] [bit] NOT NULL,
	[ComputedDbExecTime] [bigint] NULL,
	[MessageDescriptor] [int] NULL,
	[MessageSourceId] [int] NULL,
 CONSTRAINT [PK_PageThresholdHeaderTest] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC,
	[PageCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[PageThresholdHeaderTest]  WITH CHECK ADD FOREIGN KEY([MessageSourceId])
REFERENCES [dbo].[PageThresholdMessageSource] ([MessageSourceId])
GO


USE [Test]
GO

/****** Object:  Table [dbo].[PageThresholdDetailTest]    Script Date: 07/26/2012 12:43:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PageThresholdDetailTest](
	[SeqNum] [int] NOT NULL,
	[ProcName] [varchar](512) NOT NULL,
	[DataSourceAlias] [varchar](512) NOT NULL,
	[ExecutionTime] [bigint] NOT NULL,
	[ProcSeq] [int] NOT NULL,
	[BeginDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[PageCode] [tinyint] NOT NULL,
	[ComputedExecTime] [bigint] NULL,
	[CallStackId] [int] NULL,
 CONSTRAINT [PK_PageThresholdDetailTest2_1] PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC,
	[ProcSeq] ASC,
	[PageCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[PageThresholdDetailTest]  WITH CHECK ADD FOREIGN KEY([CallStackId])
REFERENCES [dbo].[PageThresholdCallStack] ([CallStack
Id])
GO

