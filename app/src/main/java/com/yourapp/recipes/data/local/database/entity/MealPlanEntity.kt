package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

/**
 * Сущность плана питания на конкретную дату.
 * Связывает рецепт с определенным днем и приемом пищи.
 */
@Entity(
    tableName = "meal_plans",
    indices = [
        Index(value = ["date"]),
        Index(value = ["recipe_id"])
    ],
    foreignKeys = [
        ForeignKey(
            entity = RecipeEntity::class,
            parentColumns = ["id"],
            childColumns = ["recipe_id"],
            onDelete = ForeignKey.CASCADE
        )
    ]
)
data class MealPlanEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "recipe_id")
    val recipeId: Long,
    
    @ColumnInfo(name = "date")
    val date: Long, // timestamp начала дня
    
    @ColumnInfo(name = "meal_type")
    val mealType: String = "dinner", // breakfast, lunch, dinner, snack
    
    @ColumnInfo(name = "servings")
    val servings: Int = 2,
    
    @ColumnInfo(name = "notes")
    val notes: String = ""
)
