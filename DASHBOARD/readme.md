# APP_DATA Dashboard 

## local development 

### SETUP 

make sure to setup the dotnet cert.

```bash
dotnet dev-certs https
```

create a .env file in the application root directory, and add the my sql connection string

```env
ASPNETCORE_Dashboard__accesskey=123456
CONNECTIONSTRINGS__SWITCH={MYSQL CONNECTION STRING}
```