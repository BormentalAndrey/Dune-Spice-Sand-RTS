package com.yourapp.recipes.data.local.dao

import androidx.room.*
import com.yourapp.recipes.data.local.database.entity.RecipeStepEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface RecipeStepDao {
    
    @Query("SELECT * FROM recipe_steps WHERE recipe_id = :recipeId ORDER BY step_number ASC")
    fun getStepsForRecipe(recipeId: Long): Flow<List<RecipeStepEntity>>
    
    @Query("SELECT * FROM recipe_steps WHERE recipe_id = :recipeId ORDER BY step_number ASC")
    suspend fun getStepsForRecipeSync(recipeId: Long): List<RecipeStepEntity>
    
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertSteps(steps: List<RecipeStepEntity>)
    
    @Update
    suspend fun updateStep(step: RecipeStepEntity)
    
    @Query("UPDATE recipe_steps SET is_completed = :isCompleted WHERE id = :stepId")
    suspend fun updateStepCompletion(stepId: Long, isCompleted: Boolean)
    
    @Query("UPDATE recipe_steps SET timer_remaining_seconds = :seconds, timer_started = :started WHERE id = :stepId")
    suspend fun updateTimer(stepId: Long, seconds: Int, started: Boolean)
    
    @Query("DELETE FROM recipe_steps WHERE recipe_id = :recipeId")
    suspend fun deleteStepsForRecipe(recipeId: Long)
    
    @Query("SELECT COUNT(*) FROM recipe_steps WHERE recipe_id = :recipeId AND is_completed = 1")
    fun getCompletedStepsCount(recipeId: Long): Flow<Int>
    
    @Query("SELECT COUNT(*) FROM recipe_steps WHERE recipe_id = :recipeId")
    fun getTotalStepsCount(recipeId: Long): Flow<Int>
}
