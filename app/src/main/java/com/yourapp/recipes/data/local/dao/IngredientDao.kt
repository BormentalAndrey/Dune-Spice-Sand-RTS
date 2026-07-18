package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.IngredientEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface IngredientDao {
    
    @Query("SELECT * FROM ingredients WHERE name LIKE '%' || :query || '%' ORDER BY usage_count DESC LIMIT 10")
    fun searchIngredients(query: String): Flow<List<IngredientEntity>>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertOrUpdateIngredient(ingredient: IngredientEntity)
    
    @Query("UPDATE ingredients SET usage_count = usage_count + 1 WHERE name = :name")
    suspend fun incrementUsageCount(name: String)
    
    @Query("SELECT * FROM ingredients ORDER BY usage_count DESC LIMIT 50")
    fun getPopularIngredients(): Flow<List<IngredientEntity>>
}
