using Famecipe.Models;
using Famecipe.Common;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Famecipe.Repository.Sqlite;

public class RecipeRepositorySqlite : IRepository<Recipe>
{
    private readonly string _recipesConnectionString;

    public RecipeRepositorySqlite()
    {
        var recipeDatabase = Environment.GetEnvironmentVariable("RECIPES_DATA_SOURCE");

        if (string.IsNullOrEmpty(recipeDatabase))
        {
            this._recipesConnectionString = $"Data Source=Recipe.db";
        }
        else
        {
            this._recipesConnectionString = $"Data Source={recipeDatabase}";
        }

        using (var connection = new SqliteConnection(this._recipesConnectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Recipe (Identifier TEXT, Name TEXT, Image TEXT, Ingredients TEXT, Equipment TEXT, Directions TEXT, WhenUpdatedUTC TEXT);
            ";
            command.ExecuteNonQuery();
        }
    }

    public async Task<Recipe> Create(Recipe recipe)
    {
        using (var connection = new SqliteConnection(this._recipesConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Recipe
                VALUES($Identifier, $Name, $Image, $Ingredients, $Equipment, $Directions, $WhenUpdatedUTC)
            ";
            command.Parameters.AddWithValue("$Identifier", recipe.Identifier);
            command.Parameters.AddWithValue("$Name", recipe.Name);
            command.Parameters.AddWithValue("$Image", recipe.Image);
            command.Parameters.AddWithValue("$Ingredients", string.Join('|', recipe.Ingredients ?? new List<string>()));
            command.Parameters.AddWithValue("$Equipment", string.Join('|', recipe.Equipment ?? new List<string>()));
            command.Parameters.AddWithValue("$Directions", string.Join('|', recipe.Directions ?? new List<string>()));
            command.Parameters.AddWithValue("$WhenUpdatedUTC", recipe.WhenUpdatedUTC!.Value.ToString("o", CultureInfo.InvariantCulture));

            await command.ExecuteNonQueryAsync();

            return await Get(recipe.Identifier!);
        }
    }

    public async Task Delete(string identifier)
    {
        using (var connection = new SqliteConnection(this._recipesConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                DELETE
                FROM Recipe
                WHERE Identifier = $Identifier
            ";
            command.Parameters.AddWithValue("$Identifier", identifier);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<IEnumerable<Recipe>> Get()
    {
        using (var connection = new SqliteConnection(this._recipesConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Recipe
            ";

            var recipes = new List<Recipe>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                var identifierOrdinal = reader.GetOrdinal("Identifier");
                var nameOrdinal = reader.GetOrdinal("Name");
                var imageOrdinal = reader.GetOrdinal("Image");
                var ingredientsOrdinal = reader.GetOrdinal("Ingredients");
                var equipmentOrdinal = reader.GetOrdinal("Equipment");
                var directionsOrdinal = reader.GetOrdinal("Directions");
                var whenUpdatedUTCOrdinal = reader.GetOrdinal("WhenUpdatedUTC");

                while (await reader.ReadAsync())
                {
                    recipes.Add(new Recipe()
                    {
                        Identifier = reader.GetString(identifierOrdinal),
                        Name = reader.GetString(nameOrdinal),
                        Image = reader.GetString(imageOrdinal),
                        Ingredients = reader.GetString(ingredientsOrdinal).Split('|').ToList(),
                        Equipment = reader.GetString(equipmentOrdinal).Split('|').ToList(),
                        Directions = reader.GetString(directionsOrdinal).Split('|').ToList(),
                        WhenUpdatedUTC = reader.GetDateTime(whenUpdatedUTCOrdinal)
                    });
                }
            }

            return recipes;
        }
    }

    public async Task<Recipe> Get(string identifier)
    {
        using (var connection = new SqliteConnection(this._recipesConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT *
                FROM Recipe
                WHERE Identifier = $Identifier
            ";
            command.Parameters.AddWithValue("$Identifier", identifier);

            Recipe recipe = null;

            using (var reader = await command.ExecuteReaderAsync())
            {
                var identifierOrdinal = reader.GetOrdinal("Identifier");
                var nameOrdinal = reader.GetOrdinal("Name");
                var imageOrdinal = reader.GetOrdinal("Image");
                var ingredientsOrdinal = reader.GetOrdinal("Ingredients");
                var equipmentOrdinal = reader.GetOrdinal("Equipment");
                var directionsOrdinal = reader.GetOrdinal("Directions");
                var whenUpdatedUTCOrdinal = reader.GetOrdinal("WhenUpdatedUTC");

                while (await reader.ReadAsync())
                {
                    recipe = new Recipe()
                    {
                        Identifier = reader.GetString(identifierOrdinal),
                        Name = reader.GetString(nameOrdinal),
                        Image = reader.GetString(imageOrdinal),
                        Ingredients = reader.GetString(ingredientsOrdinal).Split('|').ToList(),
                        Equipment = reader.GetString(equipmentOrdinal).Split('|').ToList(),
                        Directions = reader.GetString(directionsOrdinal).Split('|').ToList(),
                        WhenUpdatedUTC = reader.GetDateTime(whenUpdatedUTCOrdinal)
                    };
                }
            }

            return recipe;
        }
    }

    public async Task Update(string identifier, Recipe recipe)
    {
        using (var connection = new SqliteConnection(this._recipesConnectionString))
        {
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                UPDATE Recipe
                SET Name = $Name,
                    Image = $Image,
                    Ingredients = $Ingredients,
                    Equipment = $Equipment,
                    Directions = $Directions,
                    WhenUpdatedUTC = $WhenUpdatedUTC
                WHERE Identifier = $Identifier
            ";
            command.Parameters.AddWithValue("$Identifier", identifier);
            command.Parameters.AddWithValue("$Name", recipe.Name);
            command.Parameters.AddWithValue("$Image", recipe.Image);
            command.Parameters.AddWithValue("$Ingredients", string.Join('|', recipe.Ingredients ?? new List<string>()));
            command.Parameters.AddWithValue("$Equipment", string.Join('|', recipe.Equipment ?? new List<string>()));
            command.Parameters.AddWithValue("$Directions", string.Join('|', recipe.Directions ?? new List<string>()));
            command.Parameters.AddWithValue("$WhenUpdatedUTC", recipe.WhenUpdatedUTC!.Value.ToString("o", CultureInfo.InvariantCulture));

            await command.ExecuteNonQueryAsync();
        }
    }
}
