package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

/**
 * Сущность ингредиента для автодополнения.
 * Хранит все уникальные ингредиенты, когда-либо добавленные пользователем.
 */
@Entity(
    tableName = "ingredients",
    indices = [Index(value = ["name"], unique = true)]
)
data class IngredientEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "name")
    val name: String,
    
    @ColumnInfo(name = "category")
    val category: String = "other",
    
    @ColumnInfo(name = "usage_count")
    val usageCount: Int = 0
)
