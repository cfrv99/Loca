namespace Loca.Services.Game.Engine;

public static class TruthOrDareEngine
{
    public static readonly List<(string Type, string Category, string ContentAz)> SeedQuestions = new()
    {
        // Truth - Gülməli
        ("truth", "funny", "Ən utandığın an nə idi?"),
        ("truth", "funny", "Telefonundakı ən gülməli şəkil nədir?"),
        ("truth", "funny", "Heç sevdiyin insanın adını səhv çağırmısan?"),
        ("truth", "funny", "Ən pis yeməyi nə olub?"),
        ("truth", "funny", "Sosial mediada ən çox stalklədığın adam kimdir?"),
        ("truth", "funny", "Ən son nəyə güldüyünü xatırlayırsan?"),
        ("truth", "funny", "Ən gülməli yuxun nə idi?"),
        ("truth", "funny", "Heç ictimai yerdə utanıldığın olub?"),
        ("truth", "funny", "Ən son hansı yalanı demisən?"),
        ("truth", "funny", "Telefonunda ən gülməli mesaj hansıdır?"),

        // Truth - Romantik
        ("truth", "romantic", "İlk öpüşün necə olub?"),
        ("truth", "romantic", "İdeal randevun necə olmalıdır?"),
        ("truth", "romantic", "Bu otaqdakı birinə mesaj yazardın — kimə?"),
        ("truth", "romantic", "Ən uzun münasibətin nə qədər davam edib?"),
        ("truth", "romantic", "Sevgi ilk baxışdan olurmu?"),
        ("truth", "romantic", "İdeal partnyorun necə olmalıdır?"),
        ("truth", "romantic", "Ən romantik hədiyyə nə alıbsan?"),
        ("truth", "romantic", "Sevgiliylə ən yaxşı xatirən nədir?"),
        ("truth", "romantic", "İlk sevgin haqqında danış"),
        ("truth", "romantic", "Heç kimsəyə demədiyini birinə danışacaqsan?"),

        // Dare - Gülməli
        ("dare", "funny", "30 saniyə ərzində burada tanımadığın birinə tərif de"),
        ("dare", "funny", "Ən axırıncı axtarış tarixçəni göstər"),
        ("dare", "funny", "1 dəqiqə robot kimi danış"),
        ("dare", "funny", "Yanındakı adama sarıl"),
        ("dare", "funny", "Telefon kamerasıyla selfie çək və story-yə paylaş"),
        ("dare", "funny", "30 saniyə gözlərini yummadan dur"),
        ("dare", "funny", "Ən son aldığın mesajı yüksək səslə oxu"),
        ("dare", "funny", "10 saniyə ən gülməli üz ifadəni et"),
        ("dare", "funny", "Tanımadığın birinə 'Salam, səni haradansa tanıyıram' de"),
        ("dare", "funny", "1 dəqiqə aksent ilə danış"),

        // Dare - Ekstremal
        ("dare", "extreme", "Barmenə telefonunu ver, 1 mesaj yazsın"),
        ("dare", "extreme", "Bu oyundakı birinə 100 coin gift göndər"),
        ("dare", "extreme", "2 dəqiqə gözlərini bağla"),
        ("dare", "extreme", "Ən son zəngi kimə etmisən — indi elə ona zəng et"),
        ("dare", "extreme", "Sosial mediada ən son bəyəndiyin postu göstər"),
    };
}
