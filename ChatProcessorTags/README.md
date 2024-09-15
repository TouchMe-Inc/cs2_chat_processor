# About ChatProcessorTags

Adds chat tags that can be assigned via permission/group or SteamID64.

## Screenshots
![image](https://github.com/user-attachments/assets/4cb64635-6bf4-41f8-930d-095f0e229812)

## Configuration
```json
{
  "Tags.SteamID": {
    "76561198409XXXXX": {
      "Template": "{Green}[VIP]{TeamColor}"
    }
  },
  "Tags.Group": {
    "#css/admin": {
      "Template": "{Green}[ADMIN]{TeamColor}"
    }
  },
  "Tags.Permission": {
    "@css/chat": {
      "Template": "{Green}[CHATMASTER]{TeamColor}"
    }
  },
  "ConfigVersion": 1
}
```
