```text
MinimalApi/
├── configuration/
│   ├── ApiConfig.cs
│   ├── AppConfig.cs
│   ├── DbConnectionsConfig.cs
│   └── RedisConfig.cs
│
├── controllers/
│   ├── AuthController.cs
│   ├── CategoryController.cs
│   ├── FunkoController.cs
│   └── SupplierController.cs
│
├── dto/
│   ├── auth/
│   │   ├── AuthResponse.cs
│   │   ├── LoginRequest.cs
│   │   └── SignupRequest.cs
│   ├── categories/
│   │   ├── CategoryCreateRequestDto.cs
│   │   ├── CategoryDto.cs
│   │   └── CategoryUpdateRequestDto.cs
│   ├── funkos/
│   │   ├── FunkoCreateRequestDto.cs
│   │   ├── FunkoDto.cs
│   │   ├── FunkoUpdateCategoryRequestDto.cs
│   │   ├── FunkoUpdateRequestDto.cs
│   │   └── FunkoUpdateSupplierDto.cs
│   ├── suppliers/
│   │   ├── SupplierCreateDto.cs
│   │   ├── SupplierDto.cs
│   │   └── SupplierUpdateRequestDto.cs
│   └── PaginatedResourceDto.cs
│
├── entities/
│   ├── contextconf/
│   │   ├── MongoContextConfiguration.cs
│   │   └── PosgreContextConfiguration.cs
│   ├── AppDbContext.cs
│   └── AppDbMongoContext.cs
│
├── errors/
│   ├── AuthError.cs
│   ├── DomainError.cs
│   ├── FunkoError.cs
│   └── StorageError.cs
│
├── extensions/
│   └── DomainErrorExtensions.cs
│
├── infraestructure/
│   ├── TimeLifes/
│   │   ├── IScoped.cs
│   │   ├── ISingleton.cs
│   │   └── ITransient.cs
│   ├── AuthenticationConfig.cs
│   ├── DatabaseConfig.cs
│   ├── InitializeDb.cs
│   ├── RedisConnectionConfig.cs
│   ├── RepositoriesConfig.cs
│   ├── ServicesConfiguration.cs
│   └── StorageConfig.cs
│
├── mappers/
│   └── FunkoMapper.cs
│
├── middleware/
│   └── GlobalExceptionHandler.cs
│
├── models/
│   ├── Category.cs
│   ├── Funko.cs
│   ├── Role.cs
│   ├── Supplier.cs
│   └── User.cs
│
├── repositories/
│   ├── Categories/
│   │   ├── CategoryRepository.cs
│   │   └── ICategoryRepository.cs
│   ├── funkos/
│   │   ├── FunkoRepository.cs
│   │   └── IFunkoRepository.cs
│   ├── suppliers/
│   │   ├── ISuppliersRepository.cs
│   │   └── SupplierRepository.cs
│   └── users/
│       ├── IUserRepository.cs
│       └── UserRepository.cs
│
├── services/
│   ├── cache/
│   │   ├── ICache.cs
│   │   └── RedisCache.cs
│   ├── categories/
│   │   ├── CategoryService.cs
│   │   └── ICategoryService.cs
│   ├── funkos/
│   │   ├── FunkoService.cs
│   │   └── IFunkoService.cs
│   ├── jwt/
│   │   ├── AuthService.cs
│   │   ├── IAuthService.cs
│   │   ├── IJwtService.cs
│   │   └── JwtService.cs
│   └── suppliers/
│       ├── ISuppliersService.cs
│       └── SuppliersService.cs
│
├── storage/
│   ├── FileSystemStorageService.cs
│   └── IStorageService.cs
│
├── validators/
│   ├── dtos/
│   │   ├── ObjectIdAttribute.cs
│   │   └── YoutubeIdAttribute.cs
│   └── fluent/
│       └── YoutubeIdValueGenerator.cs
│
├── wwwroot/
│   └── uploads/
│       └── funkos/
│
├── appsettings.json
├── appsettings.Postgre.json
├── docker-compose.yml
├── Dockerfile
├── MinimalApi.csproj
├── MinimalApi.http
└── Program.cs