package com.yourapp.recipes.domain.repository

import com.yourapp.recipes.domain.model.*
import kotlinx.coroutines.flow.Flow

interface RecipeRepository {
    fun getFilteredRecipes(filter: RecipeFilter): Flow<List<Recipe>>
    suspend fun getRecipeById(recipeId: Long): Recipe?
    fun getRecipeByIdFlow(recipeId: Long): Flow<Recipe?>
    suspend fun getRandomRecipe(): Recipe?
    fun getFavoriteRecipes(): Flow<List<Recipe>>
    suspend fun saveRecipe(recipe: Recipe): Long
    suspend fun deleteRecipe(recipeId: Long)
    suspend fun updateFavoriteStatus(recipeId: Long, isFavorite: Boolean)
    fun searchIngredients(query: String): Flow<List<String>>
    suspend fun getRecipeCount(): Int
}
