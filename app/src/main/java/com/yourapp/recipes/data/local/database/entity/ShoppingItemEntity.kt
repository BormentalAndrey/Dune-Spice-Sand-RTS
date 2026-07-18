package com.yourapp.recipes.data.local.database.entity

import androidx.room.*

/**
 * Сущность элемента списка покупок.
 * Может быть создана из рецепта или добавлена вручную.
 */
@Entity(
    tableName = "shopping_items",
    indices = [
        Index(value = ["category"]),
        Index(value = ["is_purchased"])
    ]
)
data class ShoppingItemEntity(
    @PrimaryKey(autoGenerate = true)
    val id: Long = 0,
    
    @ColumnInfo(name = "name")
    val name: String,
    
    @ColumnInfo(name = "quantity")
    val quantity: Float,
    
    @ColumnInfo(name = "unit")
    val unit: String,
    
    @ColumnInfo(name = "category")
    val category: String = "other",
    
    @ColumnInfo(name = "is_purchased")
    val isPurchased: Boolean = false,
    
    @ColumnInfo(name = "recipe_id")
    val recipeId: Long? = null,
    
    @ColumnInfo(name = "date_added")
    val dateAdded: Long = System.currentTimeMillis()
)
