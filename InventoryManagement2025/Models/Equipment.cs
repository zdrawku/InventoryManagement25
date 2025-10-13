using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using InventoryManagement2025.Models;

namespace InventoryManagement2025.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Type { get; set; }

        public string SerialNumber { get; set; }

        // 1️⃣ Equipment condition enumeration
        public Condition Condition { get; set; }

        // 2️⃣ Equipment status enumeration
        public EquipmentStatus Status { get; set; }

        // 3️⃣ Location as free text (e.g., "Room 204", "Library")
        public string Location { get; set; }

        public string PhotoUrl { get; set; }

        // Navigation property
       // public ICollection<Request> Requests { get; set; }
    }

    // --- ENUM DEFINITIONS BELOW ---

    // Represents the physical condition of the equipment
    public enum Condition
    {
        Excellent = 1,
        Good = 2,
        Fair = 3,
        Damaged = 4,
    }

    // Represents the availability or operational status
    public enum EquipmentStatus
    {
        Available = 1,
        Unavailable = 2,
        UnderRepair = 3,
    }


public static class EquipmentEndpoints
{
	public static void MapEquipmentEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Equipment").WithTags(nameof(Equipment));

        group.MapGet("/", async (SchoolInventory db) =>
        {
            return await db.Equipments.ToListAsync();
        })
        .WithName("GetAllEquipment")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<Equipment>, NotFound>> (int equipmentid, SchoolInventory db) =>
        {
            return await db.Equipments.AsNoTracking()
                .FirstOrDefaultAsync(model => model.EquipmentId == equipmentid)
                is Equipment model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetEquipmentById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int equipmentid, Equipment equipment, SchoolInventory db) =>
        {
            var affected = await db.Equipments
                .Where(model => model.EquipmentId == equipmentid)
                .ExecuteUpdateAsync(setters => setters
                  .SetProperty(m => m.EquipmentId, equipment.EquipmentId)
                  .SetProperty(m => m.Name, equipment.Name)
                  .SetProperty(m => m.Type, equipment.Type)
                  .SetProperty(m => m.SerialNumber, equipment.SerialNumber)
                  .SetProperty(m => m.Condition, equipment.Condition)
                  .SetProperty(m => m.Status, equipment.Status)
                  .SetProperty(m => m.Location, equipment.Location)
                  .SetProperty(m => m.PhotoUrl, equipment.PhotoUrl)
                  );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateEquipment")
        .WithOpenApi();

        group.MapPost("/", async (Equipment equipment, SchoolInventory db) =>
        {
            db.Equipments.Add(equipment);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Equipment/{equipment.EquipmentId}",equipment);
        })
        .WithName("CreateEquipment")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int equipmentid, SchoolInventory db) =>
        {
            var affected = await db.Equipments
                .Where(model => model.EquipmentId == equipmentid)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteEquipment")
        .WithOpenApi();
    }
}}
