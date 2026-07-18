package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

@Entity(
    tableName = "recipes",
    indices = [
        Index(value = ["title"]),
        Index(value = ["category"]),
        Index(value = ["difficulty"]),
        Index(value = ["cooking_time_minutes"]),
        Index(value = ["is_favorite"])
    ]
)
data class RecipeEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "title")
    val title: String,
    
    @ColumnInfo(name = "description")
    val description: String = "",
    
    @ColumnInfo(name = "cooking_time_minutes")
    val cookingTimeMinutes: Int = 0,
    
    @ColumnInfo(name = "difficulty")
    val difficulty: String = "EASY",
    
    @ColumnInfo(name = "category")
    val category: String = "DINNER",
    
    @ColumnInfo(name = "ingredients_json")
    val ingredientsJson: String = "[]",
    
    @ColumnInfo(name = "steps_json")
    val stepsJson: String = "[]",
    
    @ColumnInfo(name = "calories")
    val calories: Float = 0f,
    
    @ColumnInfo(name = "proteins")
    val proteins: Float = 0f,
    
    @ColumnInfo(name = "fats")
    val fats: Float = 0f,
    
    @ColumnInfo(name = "carbohydrates")
    val carbohydrates: Float = 0f,
    
    @ColumnInfo(name = "servings")
    val servings: Int = 2,
    
    @ColumnInfo(name = "photo_path")
    val photoPath: String? = null,
    
    @ColumnInfo(name = "is_favorite")
    val isFavorite: Boolean = false,
    
    @ColumnInfo(name = "date_added")
    val dateAdded: Long = System.currentTimeMillis(),
    
    @ColumnInfo(name = "date_modified")
    val dateModified: Long = System.currentTimeMillis()
)
