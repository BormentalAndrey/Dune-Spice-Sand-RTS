// app/src/main/java/com/yourapp/recipes/data/local/RecipeJsonLoader.kt

package com.yourapp.recipes.data.local

import android.content.Context
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.yourapp.recipes.data.local.dao.IngredientDao
import com.yourapp.recipes.data.local.dao.RecipeDao
import com.yourapp.recipes.data.local.database.entity.IngredientEntity
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.BufferedReader
import java.io.InputStreamReader
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class RecipeJsonLoader @Inject constructor(
    @ApplicationContext private val context: Context,
    private val recipeDao: RecipeDao,
    private val ingredientDao: IngredientDao,
    private val gson: Gson
) {
    
    data class JsonRecipe(
        val title: String,
        val description: String = "",
        val cookingTimeMinutes: Int = 0,
        val difficulty: String = "EASY",
        val category: String = "DINNER",
        val ingredients: List<JsonIngredient> = emptyList(),
        val steps: List<JsonStep> = emptyList(),
        val calories: Float = 0f,
        val proteins: Float = 0f,
        val fats: Float = 0f,
        val carbohydrates: Float = 0f,
        val servings: Int = 2
    )
    
    data class JsonIngredient(
        val name: String,
        val quantity: Float,
        val unit: String,
        val category: String = "other"
    )
    
    data class JsonStep(
        val stepNumber: Int,
        val description: String,
        val durationMinutes: Int = 0
    )
    
    suspend fun loadRecipesFromAssets(): Int {
        return withContext(Dispatchers.IO) {
            try {
                val jsonString = context.assets.open("recipes.json")
                    .bufferedReader()
                    .use { it.readText() }
                
                val type = object : TypeToken<Map<String, Any>>() {}.type
                val data: Map<String, Any> = gson.fromJson(jsonString, type)
                val recipesList = data["recipes"] as? List<Map<String, Any>> ?: return@withContext 0
                
                var loadedCount = 0
                val batchSize = 500
                
                recipesList.chunked(batchSize).forEach { batch ->
                    val entities = batch.mapNotNull { recipeMap ->
                        try {
                            val recipe = gson.fromJson(
                                gson.toJson(recipeMap),
                                JsonRecipe::class.java
                            )
                            convertToEntity(recipe)
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
            calories = recipe.calories,
            proteins = recipe.proteins,
            fats = recipe.fats,
            carbohydrates = recipe.carbohydrates,
            servings = recipe.servings
        )
    }
}
