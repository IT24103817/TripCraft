using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <summary>The Admin Settings page can choose Gemini, the hosted LLM provider.</summary>
    public partial class AllowGeminiProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_app_settings_llm_provider",
                table: "app_settings");

            migrationBuilder.AddCheckConstraint(
                name: "ck_app_settings_llm_provider",
                table: "app_settings",
                sql: "llm_provider IN ('ollama', 'gemini', 'groq')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The old constraint has no Gemini: a saved Gemini choice falls back to Ollama.
            migrationBuilder.Sql("UPDATE app_settings SET llm_provider = 'ollama' WHERE llm_provider = 'gemini';");
            migrationBuilder.DropCheckConstraint(
                name: "ck_app_settings_llm_provider",
                table: "app_settings");

            migrationBuilder.AddCheckConstraint(
                name: "ck_app_settings_llm_provider",
                table: "app_settings",
                sql: "llm_provider IN ('ollama', 'groq')");
        }
    }
}
