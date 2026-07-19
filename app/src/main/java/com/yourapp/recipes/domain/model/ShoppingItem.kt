package com.yourapp.recipes.domain.model

data class ShoppingItem(
    val id: Long = 0,
    val name: String,
    val quantity: Float,
    val unit: String,
    val category: String = "other",
    val isPurchased: Boolean = false,
    val recipeId: Long? = null,
    val price: Float = 0f
) {
    val displayQuantity: String
        get() = if (quantity == quantity.toLong().toFloat()) {
            quantity.toLong().toString()
        } else {
            String.format("%.1f", quantity)
        }
    
    val displayText: String
        get() = "$displayQuantity $unit $name"
    
    val totalPrice: Float
        get() = price * quantity
}
