﻿using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.Collections;
using Recollectable.Core.Entities.Locations;
using Recollectable.Core.Entities.Users;
using Recollectable.Infrastructure.Data;
using System;
using System.Linq;

namespace Recollectable.Tests
{
    public class RecollectableInitializer
    {
        public static void Initialize(RecollectableContext context)
        {
            if (context.Users.Any() || context.Countries.Any())
            {
                return;
            }

            Seed(context);
        }

        private static void Seed(RecollectableContext context)
        {
            var users = new[]
            {
                new User
                {
                    Id = new Guid("4a9522da-66f9-4dfb-88b8-f92b950d1df1"),
                    FirstName = "Ryan",
                    LastName = "Haywood",
                    UserName = "Ryan"
                },
                new User
                {
                    Id = new Guid("c7304af2-e5cd-4186-83d9-77807c9512ec"),
                    FirstName = "Michael",
                    LastName = "Jones",
                    UserName = "Michael"
                },
                new User
                {
                    Id = new Guid("e640b01f-9eb8-407f-a8f9-68197a7fe48e"),
                    FirstName = "Geoff",
                    LastName = "Ramsey",
                    UserName = "Geoff"
                },
                new User
                {
                    Id = new Guid("2e795c80-8c60-4d18-bd10-ca5832ab4158"),
                    FirstName = "Jack",
                    LastName = "Pattillo",
                    UserName = "Jack"
                },
                new User
                {
                    Id = new Guid("ca26fdfb-46b3-4120-9e52-a07820bc0409"),
                    FirstName = "Jeremy",
                    LastName = "Dooley",
                    UserName = "Jeremy"
                },
                new User
                {
                    Id = new Guid("58ba1e18-46a2-44d5-8f88-51a8e6426a56"),
                    FirstName = "Gavin",
                    LastName = "Free",
                    UserName = "Gavin"
                }
            };

            var collections = new[]
            {
                new Collection
                {
                    Id = new Guid("03a6907d-4e93-4863-bdaf-1d05140dec12"),
                    Type = "Coin",
                    UserId = new Guid("4a9522da-66f9-4dfb-88b8-f92b950d1df1")
                },
                new Collection
                {
                    Id = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789"),
                    Type = "Coin",
                    UserId = new Guid("e640b01f-9eb8-407f-a8f9-68197a7fe48e")
                },
                new Collection
                {
                    Id = new Guid("80fa9706-2465-48cf-8933-932fdce18c89"),
                    Type = "Banknote",
                    UserId = new Guid("c7304af2-e5cd-4186-83d9-77807c9512ec")
                },
                new Collection
                {
                    Id = new Guid("528fc017-4289-492a-b942-bb34a2363d9d"),
                    Type = "Banknote",
                    UserId = new Guid("2e795c80-8c60-4d18-bd10-ca5832ab4158")
                },
                new Collection
                {
                    Id = new Guid("6ee10276-5cb7-4c9f-819d-9204274c088a"),
                    Type = "Banknote",
                    UserId = new Guid("4a9522da-66f9-4dfb-88b8-f92b950d1df1")
                },
                new Collection
                {
                    Id = new Guid("ab76b149-09c9-40c8-9b35-e62e53e06c8a"),
                    Type = "Coin",
                    UserId = new Guid("c7304af2-e5cd-4186-83d9-77807c9512ec")
                }
            };

            var countries = new[]
            {
                new Country
                {
                    Id = new Guid("c8f2031e-c780-4d27-bf13-1ee48a7207a3"),
                    Name = "United States of America"
                },
                new Country
                {
                    Id = new Guid("1e6a79fa-f216-41a4-8efe-0b87e58d2b33"),
                    Name = "Kuwait"
                },
                new Country
                {
                    Id = new Guid("74619fd9-898c-4250-b5c9-833ce2d599c0"),
                    Name = "Canada"
                },
                new Country
                {
                    Id = new Guid("8c29c8a2-93ae-483d-8235-b0c728d3a034"),
                    Name = "Mexico"
                },
                new Country
                {
                    Id = new Guid("1b38bfce-567c-4d49-9dd2-e0fbef480367"),
                    Name = "France"
                },
                new Country
                {
                    Id = new Guid("8cef5964-01a4-40c7-9f16-28af109094d4"),
                    Name = "Japan"
                }
            };

            var collectorValues = new[]
            {
                new CollectorValue
                {
                    Id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5"),
                    G4 = 15.54,
                    VG8 = 15.54,
                    F12 = 15.54,
                    VF20 = 15.54,
                    XF40 = 25,
                    MS60 = 28,
                    MS63 = 32
                },
                new CollectorValue
                {
                    Id = new Guid("46bac791-8afc-420f-975e-3f3b5f3778fb"),
                    PF60 = 50,
                    PF63 = 65,
                    PF65 = 85
                },
                new CollectorValue
                {
                    Id = new Guid("2c716f5b-6792-4753-9f1a-fa8bcd4eabfb"),
                    G4 = 3,
                    VG8 = 3.50,
                    F12 = 4,
                    VF20 = 4.50,
                    XF40 = 13.50,
                    MS60 = 40,
                    MS63 = 165
                },
                new CollectorValue
                {
                    Id = new Guid("64246e79-c3fe-4020-a222-32c0f329a643"),
                    G4 = 10,
                    VG8 = 25,
                    F12 = 32,
                    VF20 = 55,
                    XF40 = 125,
                    MS60 = 200,
                    MS63 = 250
                },
                new CollectorValue
                {
                    Id = new Guid("2037c78d-81cd-45c6-b447-476cc1ba90a4"),
                    G4 = 125.48,
                    VG8 = 25,
                    F12 = 32,
                    VF20 = 55,
                    XF40 = 125,
                    MS60 = 285,
                    MS63 = 320,
                    PF60 = 350,
                    PF63 = 375,
                    PF65 = 425
                },
                new CollectorValue
                {
                    Id = new Guid("5e9cb33b-b12c-4e20-8113-d8e002aeb38d"),
                    G4 = 760,
                    VG8 = 760,
                    F12 = 760,
                    VF20 = 760,
                    XF40 = 760,
                    MS60 = 1650,
                    MS63 = 1650
                }
            };

            var coins = new[]
            {
                new Coin
                {
                    Id = new Guid("a4b0f559-449f-414c-943e-5e69b6c522fb"),
                    Type = "Dollars",
                    CountryId = new Guid("c8f2031e-c780-4d27-bf13-1ee48a7207a3"),
                    Country = countries[0],
                    CollectorValueId = new Guid("2c716f5b-6792-4753-9f1a-fa8bcd4eabfb"),
                    CollectorValue = collectorValues[2]
                },
                new Coin
                {
                    Id = new Guid("3a7fd6a5-d654-4647-8374-eba27001b0d3"),
                    Type = "Pesos",
                    CountryId = new Guid("8c29c8a2-93ae-483d-8235-b0c728d3a034"),
                    Country = countries[3],
                    CollectorValueId = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5"),
                    CollectorValue = collectorValues[0]
                },
                new Coin
                {
                    Id = new Guid("be258d41-f9f5-46d3-9738-f9e0123201ac"),
                    Type = "Pounds",
                    CountryId = new Guid("74619fd9-898c-4250-b5c9-833ce2d599c0"),
                    Country = countries[2],
                    CollectorValueId = new Guid("64246e79-c3fe-4020-a222-32c0f329a643"),
                    CollectorValue = collectorValues[3]
                },
                new Coin
                {
                    Id = new Guid("dc94e4a0-8ad1-4eec-ad9d-e4c6cf147f48"),
                    Type = "Euros",
                    CountryId = new Guid("1b38bfce-567c-4d49-9dd2-e0fbef480367"),
                    Country = countries[4],
                    CollectorValueId = new Guid("46bac791-8afc-420f-975e-3f3b5f3778fb"),
                    CollectorValue = collectorValues[1]
                },
                new Coin
                {
                    Id = new Guid("db14f24e-aceb-4315-bfcf-6ace1f9b3613"),
                    Type = "Yen",
                    CountryId = new Guid("8cef5964-01a4-40c7-9f16-28af109094d4"),
                    Country = countries[5],
                    CollectorValueId = new Guid("2c716f5b-6792-4753-9f1a-fa8bcd4eabfb"),
                    CollectorValue = collectorValues[2]
                },
                new Coin
                {
                    Id = new Guid("30a24244-ca29-40a8-95a6-8f68f5de2f78"),
                    Type = "Dime",
                    CountryId = new Guid("c8f2031e-c780-4d27-bf13-1ee48a7207a3"),
                    Country = countries[0],
                    CollectorValueId = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5"),
                    CollectorValue = collectorValues[0]
                }
            };

            var banknotes = new[]
            {
                new Banknote
                {
                    Id = new Guid("54826cab-0395-4304-8c2f-6c3bdc82237f"),
                    Type = "Dollars",
                    CountryId = new Guid("c8f2031e-c780-4d27-bf13-1ee48a7207a3"),
                    Country = countries[0],
                    CollectorValueId = new Guid("2037c78d-81cd-45c6-b447-476cc1ba90a4"),
                    CollectorValue = collectorValues[4]
                },
                new Banknote
                {
                    Id = new Guid("28c83ea6-665c-41a0-acb0-92a057228fd4"),
                    Type = "Pesos",
                    CountryId = new Guid("8c29c8a2-93ae-483d-8235-b0c728d3a034"),
                    Country = countries[3],
                    CollectorValueId = new Guid("46bac791-8afc-420f-975e-3f3b5f3778fb"),
                    CollectorValue = collectorValues[1]
                },
                new Banknote
                {
                    Id = new Guid("51d91016-54f5-44f0-a1d8-e87f72d4bcc4"),
                    Type = "Yen",
                    CountryId = new Guid("8cef5964-01a4-40c7-9f16-28af109094d4"),
                    Country = countries[5],
                    CollectorValueId = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5"),
                    CollectorValue = collectorValues[0]
                },
                new Banknote
                {
                    Id = new Guid("48d9049b-04f0-4c24-a1c3-c3668878013e"),
                    Type = "Dollars",
                    CountryId = new Guid("c8f2031e-c780-4d27-bf13-1ee48a7207a3"),
                    Country = countries[0],
                    CollectorValueId = new Guid("46bac791-8afc-420f-975e-3f3b5f3778fb"),
                    CollectorValue = collectorValues[1]
                },
                new Banknote
                {
                    Id = new Guid("3da0c34f-dbfb-41a3-801f-97b7f4cdde89"),
                    Type = "Pounds",
                    CountryId = new Guid("74619fd9-898c-4250-b5c9-833ce2d599c0"),
                    Country = countries[2],
                    CollectorValueId = new Guid("5e9cb33b-b12c-4e20-8113-d8e002aeb38d"),
                    CollectorValue = collectorValues[5]
                },
                new Banknote
                {
                    Id = new Guid("0acf8863-1bec-49a6-b761-ce27dd219e7c"),
                    Type = "Dinars",
                    CountryId = new Guid("1e6a79fa-f216-41a4-8efe-0b87e58d2b33"),
                    Country = countries[1],
                    CollectorValueId = new Guid("64246e79-c3fe-4020-a222-32c0f329a643"),
                    CollectorValue = collectorValues[3]
                }
            };

            var collectables = new[]
            {
                new CollectionCollectable
                {
                    Id = new Guid("355e785b-dd47-4fb7-b112-1fb34d189569"),
                    CollectionId = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789"),
                    CollectableId = new Guid("a4b0f559-449f-414c-943e-5e69b6c522fb"),
                    ConditionId = new Guid("e55b0420-f390-41e6-9100-212b611bbca7")
                },
                new CollectionCollectable
                {
                    Id = new Guid("88460c77-e98b-403a-8e15-1a26d843ffe5"),
                    CollectionId = new Guid("6ee10276-5cb7-4c9f-819d-9204274c088a"),
                    CollectableId = new Guid("51d91016-54f5-44f0-a1d8-e87f72d4bcc4"),
                    ConditionId = new Guid("58f7b2c7-b8fc-48dc-83ab-862a85c80fc8")
                },
                new CollectionCollectable
                {
                    Id = new Guid("ba0d7466-1fe9-478e-803c-0fcbcd48f6ec"),
                    CollectionId = new Guid("80fa9706-2465-48cf-8933-932fdce18c89"),
                    CollectableId = new Guid("28c83ea6-665c-41a0-acb0-92a057228fd4"),
                    ConditionId = new Guid("b064b098-d141-4935-ac7e-b78a1063fc6d")
                },
                new CollectionCollectable
                {
                    Id = new Guid("22e513a9-b851-4b93-931c-5904d9120f73"),
                    CollectionId = new Guid("ab76b149-09c9-40c8-9b35-e62e53e06c8a"),
                    CollectableId = new Guid("db14f24e-aceb-4315-bfcf-6ace1f9b3613"),
                    ConditionId = new Guid("c48c174e-96dd-4eef-9e79-2e6f67446344")
                },
                new CollectionCollectable
                {
                    Id = new Guid("c165ebe2-3b35-4eeb-9fab-5f952598a0c5"),
                    CollectionId = new Guid("80fa9706-2465-48cf-8933-932fdce18c89"),
                    CollectableId = new Guid("0acf8863-1bec-49a6-b761-ce27dd219e7c"),
                    ConditionId = new Guid("371da3ae-d2e0-4ee7-abf3-3a7574ae669a")
                },
                new CollectionCollectable
                {
                    Id = new Guid("25da5d7a-d9bc-4f31-9982-2a44d1facdb1"),
                    CollectionId = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789"),
                    CollectableId = new Guid("3a7fd6a5-d654-4647-8374-eba27001b0d3"),
                    ConditionId = new Guid("64dc0403-db60-479a-bce4-8662e3a16e55")
                }
            };

            var conditions = new[]
            {
                new Condition
                {
                    Id = new Guid("b064b098-d141-4935-ac7e-b78a1063fc6d"),
                    Grade = "Fine",
                    LanguageCode = "en-GB"
                },
                new Condition
                {
                    Id = new Guid("c48c174e-96dd-4eef-9e79-2e6f67446344"),
                    Grade = "Good",
                    LanguageCode = "en-GB"
                },
                new Condition
                {
                    Id = new Guid("64dc0403-db60-479a-bce4-8662e3a16e55"),
                    Grade = "VG10",
                    LanguageCode = "en-US"
                },
                new Condition
                {
                    Id = new Guid("e55b0420-f390-41e6-9100-212b611bbca7"),
                    Grade = "XF45",
                    LanguageCode = "en-US"
                },
                new Condition
                {
                    Id = new Guid("371da3ae-d2e0-4ee7-abf3-3a7574ae669a"),
                    Grade = "AU52",
                    LanguageCode = "en-US"
                },
                new Condition
                {
                    Id = new Guid("58f7b2c7-b8fc-48dc-83ab-862a85c80fc8"),
                    Grade = "MS68",
                    LanguageCode = "en-US"
                },
            };

            context.Users.AddRange(users);
            context.Collections.AddRange(collections);
            context.Coins.AddRange(coins);
            context.Banknotes.AddRange(banknotes);
            context.Countries.AddRange(countries);
            context.CollectorValues.AddRange(collectorValues);
            context.AddRange(collectables);
            context.Conditions.AddRange(conditions);
            context.SaveChanges();
        }
    }
}