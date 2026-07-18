package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.RecipeEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface RecipeDao {
    
    @Query("""
        SELECT * FROM recipes 
        WHERE (:searchQuery = '' OR title LIKE '%' || :searchQuery || '%' 
               OR description LIKE '%' || :searchQuery || '%')
        AND (:category IS NULL OR category = :category)
        AND (:difficulty IS NULL OR difficulty = :difficulty)
        AND (:maxCookingTime IS NULL OR cooking_time_minutes <= :maxCookingTime)
        AND (:onlyFavorites = 0 OR is_favorite = 1)
        ORDER BY 
            CASE WHEN :sortBy = 'TITLE' THEN title END ASC,
            CASE WHEN :sortBy = 'COOKING_TIME' THEN cooking_time_minutes END ASC,
            CASE WHEN :sortBy = 'CALORIES' THEN calories END ASC,
            date_added DESC
    """)
    fun getFilteredRecipes(
        searchQuery: String = "",
        category: String? = null,
        difficulty: String? = null,
        maxCookingTime: Int? = null,
        onlyFavorites: Boolean = false,
        sortBy: String = "DATE_ADDED"
    ): Flow<List<RecipeEntity>>
    
    @Query("SELECT * FROM recipes WHERE id = :recipeId")
    suspend fun getRecipeById(recipeId: Long): RecipeEntity?
    
    @Query("SELECT * FROM recipes WHERE id = :recipeId")
    fun getRecipeByIdFlow(recipeId: Long): Flow<RecipeEntity?>
    
    @Query("SELECT * FROM recipes WHERE is_favorite = 1")
    fun getFavoriteRecipes(): Flow<List<RecipeEntity>>
    
    @Query("""
        SELECT * FROM recipes 
        WHERE ingredients_json LIKE '%' || :ingredient || '%'
    """)
    fun searchByIngredient(ingredient: String): Flow<List<RecipeEntity>>
    
    @Query("SELECT * FROM recipes ORDER BY RANDOM() LIMIT 1")
    suspend fun getRandomRecipe(): RecipeEntity?
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertRecipe(recipe: RecipeEntity): Long
    
    @Update
    suspend fun updateRecipe(recipe: RecipeEntity)
    
    @Query("DELETE FROM recipes WHERE id = :recipeId")
    suspend fun deleteRecipe(recipeId: Long)
    
    @Query("UPDATE recipes SET is_favorite = :isFavorite WHERE id = :recipeId")
    suspend fun updateFavoriteStatus(recipeId: Long, isFavorite: Boolean)
    
    @Query("SELECT COUNT(*) FROM recipes")
    suspend fun getRecipeCount(): Int
}
