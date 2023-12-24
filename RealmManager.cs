using Realms;
// ReSharper disable CommentTypo StringLiteralTypo

namespace SonyHeadphones;

public static class RealmManager
{
    public const string RealmPath = "/home/casper/Downloads/sony_realm/sonyheadphones.realm";
    public static Realm GetInstance()
    {
        

        RealmConfiguration config = new RealmConfiguration(RealmPath)
        {
            SchemaVersion = 3
        };

        return Realm.GetInstance(config);
    }
}