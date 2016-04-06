USE [Test]
GO

/****** Object:  Table [dbo].[EDHIDdbCallDetails]    Script Date: 07/30/2012 14:03:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[EDHIDdbCallDetails](
	[SeqNum] [int] NOT NULL,
	[StoredProcName] [varchar](512) NOT NULL,
	[ExecutionTime] [bigint] NOT NULL,
	[TotalTime] [bigint] NOT NULL,
	[DataSourceAlias] [varchar](512) NOT NULL,
	[BeginDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[PageCode] [smallint] NOT NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[EDHIDdbCallDetails]  WITH NOCHECK ADD  CONSTRAINT [FK_EDHIDdbCallDetails_EDHIDExceptionDetails] FOREIGN KEY([SeqNum])
REFERENCES [dbo].[EDHIDExceptionDetails] ([SeqNum])
GO

ALTER TABLE [dbo].[EDHIDdbCallDetails] CHECK CONSTRAINT [FK_EDHIDdbCallDetails_EDHIDExceptionDetails]
GO


