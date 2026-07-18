package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

/**
 * Сущность шага приготовления с таймером.
 * Хранит информацию о каждом шаге рецепта отдельно для возможности
 * отслеживания прогресса приготовления.
 */
@Entity(
    tableName = "recipe_steps",
    foreignKeys = [
        ForeignKey(
            entity = RecipeEntity::class,
            parentColumns = ["id"],
            childColumns = ["recipe_id"],
            onDelete = ForeignKey.CASCADE
        )
    ],
    indices = [Index(value = ["recipe_id"])]
)
data class RecipeStepEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "recipe_id")
    val recipeId: Long,
    
    @ColumnInfo(name = "step_number")
    val stepNumber: Int,
    
    @ColumnInfo(name = "description")
    val description: String,
    
    @ColumnInfo(name = "duration_minutes")
    val durationMinutes: Int = 0,
    
    @ColumnInfo(name = "timer_started")
    val timerStarted: Boolean = false,
    
    @ColumnInfo(name = "timer_remaining_seconds")
    val timerRemainingSeconds: Int = 0,
    
    @ColumnInfo(name = "is_completed")
    val isCompleted: Boolean = false,
    
    @ColumnInfo(name = "image_path")
    val imagePath: String? = null
)
