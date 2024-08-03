# About ChatProcessorTags

Adds tags to the server that can be assigned via permission/group or SteamID64.

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
