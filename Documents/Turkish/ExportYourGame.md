# Oyununu Dışa Aktar (Export Your Game)

Publish işlemi, kaynak proje dışında çalışabilen bir release klasörü oluşturur.
Özellikle oluşturulan projede Native AOT açıkken, komutu hedefle aynı işletim
sistemi ailesinde çalıştırın.

```bash
# Windows 64-bit
dotnet publish -c Release -r win-x64 --self-contained true

# Linux 64-bit
dotnet publish -c Release -r linux-x64 --self-contained true

# Apple Silicon
dotnet publish -c Release -r osx-arm64 --self-contained true
```

Sonuç `bin/Release/net10.0/<runtime>/publish/` altındadır. `assets` klasörü,
`Fix2Engine-LICENSE.txt` ve `Fix2Engine-THIRD-PARTY-NOTICES.txt` dahil olmak üzere
`publish` klasörünün tamamını dağıtın. Yayınlamadan önce bu kopyayı temiz bir hedef
bilgisayarda test edin.
