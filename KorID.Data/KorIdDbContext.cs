﻿using KorID.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KorID.Data;

public class KorIdDbContext : DbContext
{
    public KorIdDbContext(DbContextOptions<KorIdDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}
