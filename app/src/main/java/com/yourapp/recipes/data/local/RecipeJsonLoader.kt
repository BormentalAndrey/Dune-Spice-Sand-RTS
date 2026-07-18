package com.yourapp.recipes.data.local

import android.content.Context
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.yourapp.recipes.data.local.dao.IngredientDao
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
    
    data class JsonRecipe(
        val title: String = "",
        val description: String = "",
        val cookingTimeMinutes: Int = 0,
        val difficulty: String = "EASY",
        val category: String = "DINNER",
        val ingredients: List<Map<String, Any>> = emptyList(),
        val steps: List<Map<String, Any>> = emptyList(),
        val calories: Double = 0.0,
        val proteins: Double = 0.0,
        val fats: Double = 0.0,
        val carbohydrates: Double = 0.0,
        val servings: Int = 2
    )
    
    suspend fun loadRecipesFromAssets(): Int = withContext(Dispatchers.IO) {
        try {
            val jsonString = context.assets.open("recipes.json")
                .bufferedReader()
                .use { it.readText() }
            
            val type = object : TypeToken<Map<String, Any>>() {}.type
            val data: Map<String, Any> = gson.fromJson(jsonString, type)
            
            @Suppress("UNCHECKED_CAST")
            val recipesList = data["recipes"] as? List<Map<String, Any>> ?: return@withContext 0
            
            var loadedCount = 0
            val batchSize = 100
            
            recipesList.chunked(batchSize).forEach { batch ->
                val entities = batch.mapNotNull { recipeMap ->
                    try {
                        val recipeJson = gson.fromJson(
                            gson.toJson(recipeMap),
                            JsonRecipe::class.java
                        )
                        convertToEntity(recipeJson)
                    } catch (e: Exception) {
                        null
                    }
                }
                
                if (entities.isNotEmpty()) {
                    recipeDao.insertRecipes(entities)
                    loadedCount += entities.size
                }
            }
            
            loadedCount
        } catch (e: Exception) {
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
