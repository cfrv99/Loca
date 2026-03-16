# Loca — Azerbaijani Content Specification

## UI Strings (use these EXACT translations)
```
Discover = "Kəşf et"
Hub = "Hub"  
Matches = "Matçlar"
Profile = "Profil"
Login = "Daxil ol"
Register = "Qeydiyyat"
Search = "Axtar"
Send = "Göndər"
Cancel = "Ləğv et"
Save = "Saxla"
Next = "Növbəti"
Back = "Geri"
Done = "Hazır"
Loading = "Yüklənir..."
Error = "Xəta baş verdi"
Retry = "Yenidən cəhd et"
No results = "Nəticə tapılmadı"
Pull to refresh = "Yeniləmək üçün aşağı çəkin"

// Venue
Check in = "QR Scan et"
People here = "{n} nəfər"
Active games = "{n} aktiv oyun"
Chat hidden teaser = "Chat-da nə baş verir? QR scan et və qoşul!"

// Chat
Type message = "Mesaj yaz..."
Reply = "Cavab ver"
Voice message = "Səs mesajı"

// Games
Create game = "Oyun yarat"
Join game = "Qoşul"
Start game = "Başla"
Your turn = "Sənin növbəndir"
Waiting = "Gözlənilir..."
Game over = "Oyun bitdi"
Winner = "Qalib"
You won = "Təbrik edirik! Qalib oldun! 🎉"
You lost = "Təəssüf! Növbəti dəfə! 💪"

// Matching
Match request = "Tanışlıq sorğusu"
Accept = "Qəbul et"
Decline = "Rədd et"
Write intro = "Qısa mesaj yaz..."
Matched = "Uyğunlaşdınız! 🎉"

// Gifting
Coin shop = "Coin mağazası"
Send gift = "Hədiyyə göndər"
Balance = "Balans: {n} coin"
Insufficient = "Kifayət qədər coin yoxdur"

// Notifications
New message = "{name} mesaj yazdı"
Match request = "{name} tanışlıq sorğusu göndərdi"
Match accepted = "{name} qəbul etdi! Söhbətə başla"
Gift received = "{name} sənə {gift} göndərdi! 🎁"
Vibe bomb = "Kimsə sənə Vibe Bomb göndərdi! 💣"
Game starting = "{game} oyunu başlayır {venue}-da"
```

## Truth or Dare Questions (seed at least 20 per category)

### Truth - Gülməli
- "Ən utandığın an nə idi?"
- "Telefonundakı ən gülməli şəkil nədir?"
- "Heç sevdiyin insanın adını səhv çağırmısan?"
- "Ən pis yeməyi nə olub?"
- "Sosial mediada ən çox stalklədığın adam kimdir?"

### Truth - Romantik
- "İlk öpüşün necə olub?"
- "İdeal randevun necə olmalıdır?"
- "Bu otaqdakı birinə mesaj yazardın — kimə?"
- "Ən uzun münasibətin nə qədər davam edib?"
- "Sevgi ilk baxışdan olurmu?"

### Dare - Gülməli
- "30 saniyə ərzində burada tanımadığın birinə tərif de"
- "Ən axırıncı axtarış tarixçəni göstər"
- "1 dəqiqə robot kimi danış"
- "Yanındakı adama sarıl"
- "Telefon kamerasıyla selfie çək və story-yə paylaş"

### Dare - Ekstremal
- "Barmenə/ofisiantə telefonunu ver, 1 mesaj yazsın"
- "Bu oyundakı birinə 100 coin gift göndər"
- "2 dəqiqə gözlərini bağla"
- "Ən son zəngi kimə etmisən — indi elə ona zəng et"

## Quiz Categories (seed at least 50 total)

### Azərbaycan
- "Bakının ən hündür binası hansıdır?" → Flame Towers / Socar Tower / Heydar Center / TV Tower (cavab: Socar Tower)
- "Azərbaycanın paytaxtı nə vaxt elan edilib?" → 1918 / 1920 / 1991 / 2000 (cavab: 1918)

### Ümumi Bilik
- "Dünyanın ən böyük okeanı hansıdır?" → Atlantik / Sakit / Hind / Şimal Buzlu (cavab: Sakit)

## Would You Rather Pairs (seed at least 30)
- "Həmişə həqiqəti bilmək | Həmişə xoşbəxt olmaq"
- "Keçmişə səyahət | Gələcəyə səyahət"
- "Telefonunu 1 həftə yoxdur | İnterneti 1 ay yoxdur"
- "Hər mahnını bilmək | Hər dili bilmək"
- "Görünməz olmaq | Uça bilmək"

## Content Rules for Claude Code
- ALL user-facing text MUST be in Azerbaijani (AZ)
- Error messages in AZ
- Push notification text in AZ
- Game questions in AZ (EN optional secondary)
- Seed data: generate 200+ T/D questions, 500+ quiz questions, 200+ WYR pairs
- Use formal "siz" for system messages, informal "sən" for fun/game contexts
