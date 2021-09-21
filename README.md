## Ana ekran
![Alt text](static/images/1.PNG?raw=true "Ana ekran")
## Ürün ara
![Alt text](static/images/2.PNG?raw=true "Ürün ara")
## Stok esktresi getir
![Alt text](static/images/3.PNG?raw=true "Stok esktresi getir")
## Çıktı veya belge oluştur
![Alt text](static/images/4.PNG?raw=true "Çıktı veya belge oluştur")

## Stored Procedure

```sql
USE [Test]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_stok_ekstresi_olustur
	@MalKodu VARCHAR(50), 
	@BaslangicTarihi INT,
	@BitisTarihi INT
AS
BEGIN
	SET NOCOUNT ON;

	--DECLARE THE VARIABLES FOR HOLDING DATA.
	DECLARE @Miktar NUMERIC;
	DECLARE @SiraNo INT;
	DECLARE @IslemTur NVARCHAR(50);
	DECLARE @EvrakNo NVARCHAR(50);
	DECLARE @Tarih VARCHAR(15);
	DECLARE @GirisMiktar NUMERIC;
	DECLARE @CikisMiktar NUMERIC;
	DECLARE @StokMiktar NUMERIC;

	--SET VARIABLES.
	SET @SiraNo = 1;
	SET @StokMiktar = 0;

	CREATE TABLE #TempTable(
		SiraNo INT PRIMARY KEY,
		IslemTur NVARCHAR(50),
		EvrakNo NVARCHAR(50),
		Tarih NVARCHAR(50),
		GirisMiktar NUMERIC,
		CikisMiktar NUMERIC,
		StokMiktar NUMERIC,
	)

	--DECLARE THE CURSOR FOR A QUERY.
	DECLARE StokGetir CURSOR READ_ONLY
	FOR
	SELECT
		IIF(STI.IslemTur = 0, 'Giriş', 'Çıkış'),
		STI.EvrakNo,
		CONVERT(VARCHAR(15), CAST(STI.Tarih - 2 AS datetime), 104),
		IIF(STI.IslemTur = 0, STI.Miktar, 0),
		IIF(STI.IslemTur = 1, STI.Miktar, 0),
		STI.Miktar
	FROM dbo.STI
	WHERE STI.MalKodu = @MalKodu
	AND STI.Tarih >= @BaslangicTarihi
	AND STI.Tarih <= @BitisTarihi
	ORDER BY STI.Tarih ASC

	--OPEN CURSOR.
	OPEN StokGetir

	--FETCH THE RECORD INTO THE VARIABLES.
	FETCH NEXT FROM StokGetir INTO
	@IslemTur,  @EvrakNo, @Tarih, @GirisMiktar, @CikisMiktar, @Miktar

	--LOOP UNTIL RECORDS ARE AVAILABLE.
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @IslemTur = 'Giriş'
			SET @StokMiktar = @StokMiktar + @Miktar
		ELSE
			SET @StokMiktar = @StokMiktar - @Miktar
			
		--INSERT INTO TEMP TABLE
		INSERT INTO #TempTable VALUES (@SiraNo, @IslemTur, @EvrakNo, @Tarih, @GirisMiktar, @CikisMiktar, @StokMiktar)

		--INCREMENT COUNTER.
		SET @SiraNo = @SiraNo + 1

		--FETCH THE NEXT RECORD INTO THE VARIABLES.
		FETCH NEXT FROM StokGetir INTO
		@IslemTur,  @EvrakNo, @Tarih, @GirisMiktar, @CikisMiktar, @Miktar
	END

	--SELECT THE RESULTS
	SELECT * FROM #TempTable
	DROP TABLE #TempTable

	--CLOSE AND DEALLOCATE THE CURSOR.
	CLOSE StokGetir
	DEALLOCATE StokGetir
END
GO
```