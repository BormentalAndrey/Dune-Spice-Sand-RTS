package com.yourapp.recipes.data.local

import android.content.Context
import android.util.Log
import com.google.gson.Gson
import com.google.gson.annotations.SerializedName
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class RecipeJsonLoader @Inject constructor(
    @ApplicationContext private val context: Context,
    private val recipeDao: RecipeDao,
    private val gson: Gson
) {
    
    // Корневая обёртка для JSON
    data class RecipeRootResponse(
        val version: Int,
        val recipes: List<JsonRecipe>
    )
    
    // Модель рецепта из JSON
    data class JsonRecipe(
        val title: String = "",
        val description: String = "",
        val cookingTimeMinutes: Int = 0,
        val difficulty: String = "EASY",
        val category: String = "DINNER",
        val ingredients: List<JsonIngredient> = emptyList(),
        val steps: List<JsonStep> = emptyList(),
        val calories: Double = 0.0,
        val proteins: Double = 0.0,
        val fats: Double = 0.0,
        val carbohydrates: Double = 0.0,
        val servings: Int = 2
    )
    
    // Модель ингредиента из JSON
    data class JsonIngredient(
        val name: String = "",
        val quantity: Double = 0.0,
        val unit: String = "",
        val category: String = "other"
    )
    
    // Модель шага из JSON
    data class JsonStep(
        val stepNumber: Int = 0,
        val description: String = "",
        val durationMinutes: Int = 0
    )
    
    suspend fun loadRecipesFromAssets(): Int = withContext(Dispatchers.IO) {
        try {
            Log.d("JSON_LOADER", "Starting to load recipes from assets...")
            
            // Проверяем наличие файлов в assets
            val files = context.assets.list("") ?: emptyArray()
            Log.d("JSON_LOADER", "Files in assets: ${files.joinToString(", ")}")
            
            // Читаем JSON файл
            val jsonString = context.assets.open("recipes.json")
                .bufferedReader()
                .use { it.readText() }
            
            Log.d("JSON_LOADER", "JSON loaded, size: ${jsonString.length} chars")
            Log.d("JSON_LOADER", "First 300 chars: ${jsonString.take(300)}")
            
            // Парсим корневую обёртку
            val root = gson.fromJson(jsonString, RecipeRootResponse::class.java)
            Log.d("JSON_LOADER", "Parsed root: version=${root.version}, recipes count=${root.recipes.size}")
            
            var loadedCount = 0
            val batchSize = 100
            
            root.recipes.chunked(batchSize).forEach { batch ->
                val entities = batch.map { jsonRecipe ->
                    convertToEntity(jsonRecipe)
                }
                
                if (entities.isNotEmpty()) {
                    recipeDao.insertRecipes(entities)
                    loadedCount += entities.size
                    Log.d("JSON_LOADER", "Inserted batch: ${entities.size} recipes")
                }
            }
            
            Log.d("JSON_LOADER", "Total loaded: $loadedCount recipes")
            loadedCount
        } catch (e: Exception) {
            Log.e("JSON_LOADER", "Error loading recipes", e)
            e.printStackTrace()
            0
        }
    }
    
    private fun convertToEntity(recipe: JsonRecipe): RecipeEntity {
        return RecipeEntity(
            title = recipe.title,
            description = recipe.description,
            cookingTimeMinutes = recipe.cookingTimeMinutes,
            difficulty = recipe.difficulty,
            category = recipe.category,
            ingredientsJson = gson.toJson(recipe.ingredients),
            stepsJson = gson.toJson(recipe.steps),
            calories = recipe.calories.toFloat(),
            proteins = recipe.proteins.toFloat(),
            fats = recipe.fats.toFloat(),
            carbohydrates = recipe.carbohydrates.toFloat(),
            servings = recipe.servings
        )
    }
}
