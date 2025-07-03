using Zentry.Modules.ConfigurationManagement.Persistence.Enums;

namespace Zentry.Modules.ConfigurationManagement.Dtos;

public class CreateConfigurationRequest
{
    public AttributeDefinitionCreationDto? AttributeDefinitionDetails { get; set; }
    public ConfigurationCreationDto Configuration { get; set; } = new();
}
//
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "PASSWORD_COMPLEXITY",
//     "displayName": "Password Complexity Level",
//     "description": "Defines the complexity requirements for user passwords",
//     "dataType": "Selection",
//     "scopeType": "Global",
//     "unit": null,
//     "options": [
//       {
//         "value": "LOW",
//         "displayLabel": "Low Complexity",
//         "sortOrder": 1
//       },
//       {
//         "value": "MEDIUM",
//         "displayLabel": "Medium Complexity",
//         "sortOrder": 2
//       },
//       {
//         "value": "HIGH",
//         "displayLabel": "High Complexity",
//         "sortOrder": 3
//       }
//     ]
//   },
//   "configuration": {
//     "scopeType": "Global",
//     "scopeId": "12345678-1234-1234-1234-123456789012",
//     "value": "MEDIUM"
//   }
// }
//
// // Ví dụ khác cho DataType String:
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "UI_BACKGROUND_COLOR",
//     "displayName": "Background Color",
//     "description": "Default background color for the application",
//     "dataType": "String",
//     "scopeType": "Tenant",
//     "unit": null,
//     "options": null
//   },
//   "configuration": {
//     "scopeType": "Tenant",
//     "scopeId": "87654321-4321-4321-4321-210987654321",
//     "value": "#ffffff"
//   }
// }
//
// // Ví dụ cho DataType Int:
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "MAX_LOGIN_ATTEMPTS",
//     "displayName": "Maximum Login Attempts",
//     "description": "Maximum number of failed login attempts before account lockout",
//     "dataType": "Int",
//     "scopeType": "Global",
//     "unit": "attempts",
//     "options": null
//   },
//   "configuration": {
//     "scopeType": "Global",
//     "scopeId": "12345678-1234-1234-1234-123456789012",
//     "value": "5"
//   }
// }
//
// // Ví dụ cho DataType Boolean:
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "ENABLE_TWO_FACTOR_AUTH",
//     "displayName": "Enable Two-Factor Authentication",
//     "description": "Whether two-factor authentication is enabled",
//     "dataType": "Boolean",
//     "scopeType": "User",
//     "unit": null,
//     "options": null
//   },
//   "configuration": {
//     "scopeType": "User",
//     "scopeId": "11111111-1111-1111-1111-111111111111",
//     "value": "true"
//   }
// }
//
// // Ví dụ cho DataType Decimal:
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "TRANSACTION_FEE_RATE",
//     "displayName": "Transaction Fee Rate",
//     "description": "Fee rate applied to transactions",
//     "dataType": "Decimal",
//     "scopeType": "Global",
//     "unit": "percentage",
//     "options": null
//   },
//   "configuration": {
//     "scopeType": "Global",
//     "scopeId": "12345678-1234-1234-1234-123456789012",
//     "value": "2.5"
//   }
// }
//
// // Ví dụ cho DataType Date:
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "MAINTENANCE_WINDOW_START",
//     "displayName": "Maintenance Window Start Time",
//     "description": "When the maintenance window begins",
//     "dataType": "Date",
//     "scopeType": "Global",
//     "unit": null,
//     "options": null
//   },
//   "configuration": {
//     "scopeType": "Global",
//     "scopeId": "12345678-1234-1234-1234-123456789012",
//     "value": "2024-01-15T02:00:00Z"
//   }
// }
//
// // Ví dụ cho DataType Json:
// {
//   "attributeDefinitionDetails": {
//     "id": null,
//     "key": "UI_THEME_CONFIG",
//     "displayName": "UI Theme Configuration",
//     "description": "JSON configuration for UI theme settings",
//     "dataType": "Json",
//     "scopeType": "Tenant",
//     "unit": null,
//     "options": null
//   },
//   "configuration": {
//     "scopeType": "Tenant",
//     "scopeId": "87654321-4321-4321-4321-210987654321",
//     "value": "{\"primaryColor\": \"#007bff\", \"secondaryColor\": \"#6c757d\", \"darkMode\": false}"
//   }
// }
