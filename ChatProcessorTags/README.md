# About ChatProcessorTags

Adds chat tags that can be assigned via permission/group or SteamID64.

## Screenshots

![image](https://github.com/user-attachments/assets/446e5b70-0d09-49ae-9eef-3bcde6289ddd)

## Configuration
```json
{
  "Tags.SteamID": {
    "76561198409400523": {
      "Template": "__Green__[VIP]__TeamColor__ "
    }
  },
  "Tags.Group": {
    "#css/admin": {
      "Template": "__Green__[ADMIN]__TeamColor__ "
    }
  },
  "Tags.Permission": {
    "@css/chat": {
      "Template": "__Green__[CHATMASTER]__TeamColor__ "
    }
  },
  "ConfigVersion": 1
}
```
