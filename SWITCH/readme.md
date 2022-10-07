# App Data Endpoint / SWITCH

## local development 

### SETUP 

make sure to setup the dotnet cert.

```bash
dotnet dev-certs https
```

create a .env file in the application root directory, and add the my sql connection string

```env
CONNECTIONSTRINGS__SWITCH={CONNECTION STRING}
```