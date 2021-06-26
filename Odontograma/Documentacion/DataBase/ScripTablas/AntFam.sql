/****** Object:  Table [dbo].[AntFam]    Script Date: 17/07/2020 11:29:24 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AntFam](
	[AntFamId] [nvarchar](128) NOT NULL,
	[NombreCompleto] [varchar](100) NOT NULL,
	[ParentescoId] [nvarchar](128) NOT NULL,
	[EnfermedadId] [nvarchar](128) NOT NULL,
	[PacienteId] [nvarchar](128) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AntFamId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AntFam] ADD  DEFAULT (newid()) FOR [AntFamId]
GO

ALTER TABLE [dbo].[AntFam]  WITH CHECK ADD FOREIGN KEY([EnfermedadId])
REFERENCES [dbo].[Enfermedad] ([EnfermedadId])
GO

ALTER TABLE [dbo].[AntFam]  WITH CHECK ADD FOREIGN KEY([PacienteId])
REFERENCES [dbo].[Paciente] ([PacienteId])
GO

ALTER TABLE [dbo].[AntFam]  WITH CHECK ADD FOREIGN KEY([ParentescoId])
REFERENCES [dbo].[Parentesco] ([ParentescoId])
GO

